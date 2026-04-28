using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class GenerateDFRendererFeaturePass : ScriptableRenderPass
{
    public struct DFProcessingThread
    {
        public Texture sourceTexture;
        public RenderTexture targetTexture;
        public Vector2Int offset;
        public float alphaThreshold;
        public bool normalize;
        public bool invertSelection;
        public int nearestPointSearchRange;
        public int extendPixels;
        public bool boundaryDistance;
        public bool accurateDistance;
        public int shaderPropertyID;
    }

    int m_MaxThreadCount = 0;
    Stack<int> m_AvailableThreadIDs = new Stack<int>();
    Dictionary<int, DFProcessingThread> m_ProcessingThreads = new Dictionary<int, DFProcessingThread>();

    // ---------- static compute shader cache ----------
    static ComputeShader s_SdfCS;
    static int s_KernelInitSeedSC;
    static int s_KernelInitSeed;
    static int s_KernelJFA;
    static int s_KernelGetNearest;
    static int s_KernelCalcDist;
    static int s_KernelCalcDistSC;
    static bool s_Initialized;

    static readonly int ID_SourceTex        = Shader.PropertyToID("_SourceTexture");
    static readonly int ID_SingleChannelSrc = Shader.PropertyToID("_SingleChannelSourceTexture");
    static readonly int ID_PrevBuf          = Shader.PropertyToID("_PreviousBuffer");
    static readonly int ID_CurBuf           = Shader.PropertyToID("_CurrentBuffer");
    static readonly int ID_CurBufSC         = Shader.PropertyToID("_CurrentBufferSingle");
    static readonly int ID_Width            = Shader.PropertyToID("_Width");
    static readonly int ID_Height           = Shader.PropertyToID("_Height");
    static readonly int ID_IterTime         = Shader.PropertyToID("_IterationTime");
    static readonly int ID_Iter             = Shader.PropertyToID("_Iteration");
    static readonly int ID_NearestRange     = Shader.PropertyToID("_NearestPointSearchRange");
    static readonly int ID_AlphaThreshold   = Shader.PropertyToID("_AlphaThreshold");
    static readonly int ID_InvertSelection  = Shader.PropertyToID("_InvertSelection");
    static readonly int ID_MaxDistance      = Shader.PropertyToID("_MaxDistance");
    static readonly int ID_ExtendPixels     = Shader.PropertyToID("_ExtendPixels");
    static readonly int ID_AccurateDist     = Shader.PropertyToID("_AccurateDistance");
    static readonly int ID_BoundaryDist     = Shader.PropertyToID("_BoundaryDistance");
    static readonly int ID_Normalize        = Shader.PropertyToID("_Normalize");
    static readonly int ID_OffsetX          = Shader.PropertyToID("_OffsetX");
    static readonly int ID_OffsetY          = Shader.PropertyToID("_OffsetY");

    // Temp RT IDs – reused sequentially per thread (safe because each thread's block is fully flushed before the next)
    static readonly int TempID_Current      = Shader.PropertyToID("_GDFP_RT_Current");
    static readonly int TempID_Previous     = Shader.PropertyToID("_GDFP_RT_Previous");
    static readonly int TempID_Nearest      = Shader.PropertyToID("_GDFP_RT_Nearest");
    static readonly int TempID_CameraColor  = Shader.PropertyToID("_GDFP_RT_CameraColor");

    static void EnsureInitialized()
    {
        if (s_Initialized) return;
        s_SdfCS = Resources.Load<ComputeShader>("GetSDF");
        if (s_SdfCS == null) { Debug.LogError("[GenerateDFRendererFeaturePass] Cannot find GetSDF.compute"); return; }
        s_KernelInitSeedSC = s_SdfCS.FindKernel("InitializeSeedSingleChannel");
        s_KernelInitSeed   = s_SdfCS.FindKernel("InitializeSeed");
        s_KernelJFA        = s_SdfCS.FindKernel("JumpFlooding");
        s_KernelGetNearest = s_SdfCS.FindKernel("GetNearest");
        s_KernelCalcDist   = s_SdfCS.FindKernel("CalculateDistance");
        s_KernelCalcDistSC = s_SdfCS.FindKernel("CalculateDistanceSingleChannel");
        s_Initialized = true;
    }

    static bool IsSingleChannelFormat(RenderTextureFormat fmt) =>
        fmt == RenderTextureFormat.RFloat || fmt == RenderTextureFormat.RHalf ||
        fmt == RenderTextureFormat.R8     || fmt == RenderTextureFormat.R16   ||
        fmt == RenderTextureFormat.RInt;

    static bool IsInputSingleChannel(Texture tex)
    {
        if (tex is RenderTexture rt)  return IsSingleChannelFormat(rt.format);
        if (tex is Texture2D t2d)     return t2d.format == TextureFormat.RFloat  ||
                                             t2d.format == TextureFormat.RHalf   ||
                                             t2d.format == TextureFormat.R8      ||
                                             t2d.format == TextureFormat.R16     ||
                                             t2d.format == TextureFormat.Alpha8;
        return false;
    }

    // ---------- public API ----------
    public int PendingDFProcessingThread(Texture sourceTexture, RenderTexture targetTexture, Vector2Int offset, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, int extendPixels = 0, bool boundaryDistance = false, bool accurateDistance = false, int shaderPropertyID = -1)
    {
        return PendingDFProcessingThread(new DFProcessingThread
        {
            sourceTexture = sourceTexture,
            targetTexture = targetTexture,
            offset = offset,
            alphaThreshold = alphaThreshold,
            normalize = normalize,
            invertSelection = invertSelection,
            nearestPointSearchRange = nearestPointSearchRange,
            extendPixels = extendPixels,
            boundaryDistance = boundaryDistance,
            accurateDistance = accurateDistance,
            shaderPropertyID = shaderPropertyID
        });
    }

    public int PendingDFProcessingThread(DFProcessingThread thread)
    {
        if (m_AvailableThreadIDs.Count == 0)
        {
            m_MaxThreadCount++;
            m_AvailableThreadIDs.Push(m_MaxThreadCount);
        }
        int threadID = m_AvailableThreadIDs.Pop();
        m_ProcessingThreads[threadID] = thread;
        return threadID;
    }

    // 供 Feature 缓存重放时使用，指定 ID 注册
    public void PendingDFProcessingThread(int threadID, DFProcessingThread thread)
    {
        m_ProcessingThreads[threadID] = thread;
        // 确保该 ID 不会被重新分配
        m_MaxThreadCount = Mathf.Max(m_MaxThreadCount, threadID);
    }

    public void UpdateDFProcessingThread(
        int   threadID,
        Texture sourceTexture             = null,
        RenderTexture targetTexture       = null,
        Vector2Int offset                 = default,
        float alphaThreshold              = 0.5f,
        bool  normalize                   = true,
        bool  invertSelection             = false,
        int   nearestPointSearchRange     = 0,
        int   extendPixels                = 0,
        bool  boundaryDistance            = false,
        bool  accurateDistance            = false,
        int   shaderPropertyID            = -1)
    {
        if (!m_ProcessingThreads.TryGetValue(threadID, out var t)) return;

        if (sourceTexture != null) t.sourceTexture = sourceTexture;
        if (targetTexture != null) t.targetTexture = targetTexture;
        t.offset                  = offset;
        t.alphaThreshold          = alphaThreshold;
        t.normalize               = normalize;
        t.invertSelection         = invertSelection;
        t.nearestPointSearchRange = nearestPointSearchRange;
        t.extendPixels            = extendPixels;
        t.boundaryDistance        = boundaryDistance;
        t.accurateDistance        = accurateDistance;
        t.shaderPropertyID        = shaderPropertyID;

        m_ProcessingThreads[threadID] = t;
    }

    public void ReleaseDFProcessingThread(int threadID)
    {
        if (m_ProcessingThreads.ContainsKey(threadID))
        {
            m_ProcessingThreads.Remove(threadID);
            m_AvailableThreadIDs.Push(threadID);
        }
    }

    // ---------- RenderGraph ----------
    class PassData
    {
        public List<DFProcessingThread> threads = new List<DFProcessingThread>();
        public TextureHandle cameraColorHandle;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (m_ProcessingThreads.Count == 0) return;
        EnsureInitialized();
        if (!s_Initialized) return;

        using (var builder = renderGraph.AddUnsafePass<PassData>("Generate Distance Fields", out var passData))
        {
            passData.threads.Clear();
            bool needsCameraColor = false;
            foreach (var kvp in m_ProcessingThreads)
            {
                passData.threads.Add(kvp.Value);
                if (kvp.Value.sourceTexture == null) needsCameraColor = true;
            }

            if (needsCameraColor)
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                passData.cameraColorHandle = resourceData.activeColorTexture;
                builder.UseTexture(passData.cameraColorHandle, AccessFlags.Read);
            }

            builder.AllowPassCulling(false);
            builder.SetRenderFunc(static (PassData data, UnsafeGraphContext ctx) =>
            {
                CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                RTHandle cameraColorRT = data.cameraColorHandle.IsValid()
                    ? data.cameraColorHandle
                    : null;
                foreach (var thread in data.threads)
                    RecordGenerateDF(cmd, thread, cameraColorRT);
            });
        }
    }

    // ---------- per-thread DF generation (records into cmd) ----------
    static void RecordGenerateDF(CommandBuffer cmd, in DFProcessingThread t, RTHandle cameraColorRT)
    {
        RenderTexture resultRT = t.targetTexture;
        int ext   = t.extendPixels;
        int iterW = resultRT.width  + ext * 2;
        int iterH = resultRT.height + ext * 2;
        int w     = resultRT.width;
        int h     = resultRT.height;

        bool singleIn  = t.sourceTexture != null
                            ? IsInputSingleChannel(t.sourceTexture)
                            : (cameraColorRT?.rt != null && IsSingleChannelFormat(cameraColorRT.rt.format));
        bool singleOut = IsSingleChannelFormat(resultRT.format);

        int tgX = Mathf.CeilToInt(iterW / 8.0f);
        int tgY = Mathf.CeilToInt(iterH / 8.0f);

        var descARGB = new RenderTextureDescriptor(iterW, iterH, RenderTextureFormat.ARGBFloat, 0)
            { enableRandomWrite = true };

        // ── InitializeSeed ─────────────────────────────────────────────
        cmd.GetTemporaryRT(TempID_Current, descARGB);
        var curId = new RenderTargetIdentifier(TempID_Current);

        cmd.SetComputeIntParam  (s_SdfCS, ID_Width,          iterW);
        cmd.SetComputeIntParam  (s_SdfCS, ID_Height,         iterH);
        cmd.SetComputeFloatParam(s_SdfCS, ID_AlphaThreshold, t.alphaThreshold);
        cmd.SetComputeIntParam  (s_SdfCS, ID_InvertSelection, t.invertSelection ? 1 : 0);
        cmd.SetComputeIntParam  (s_SdfCS, ID_OffsetX,         t.offset.x);
        cmd.SetComputeIntParam  (s_SdfCS, ID_OffsetY,         t.offset.y);
        cmd.SetComputeIntParam  (s_SdfCS, ID_ExtendPixels,    ext);

        if (singleIn && t.sourceTexture != null)
        {
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelInitSeedSC, ID_CurBuf,          curId);
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelInitSeedSC, ID_SingleChannelSrc, t.sourceTexture);
            cmd.DispatchCompute(s_SdfCS, s_KernelInitSeedSC, tgX, tgY, 1);
        }
        else if (t.sourceTexture != null)
        {
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelInitSeed, ID_CurBuf,    curId);
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelInitSeed, ID_SourceTex, t.sourceTexture);
            cmd.DispatchCompute(s_SdfCS, s_KernelInitSeed, tgX, tgY, 1);
        }
        else
        {
            // sourceTexture == null：使用相机颜色缓冲，根据格式选 kernel
            if (cameraColorRT?.rt == null) { cmd.ReleaseTemporaryRT(TempID_Current); return; }
            if (singleIn)
            {
                cmd.SetComputeTextureParam(s_SdfCS, s_KernelInitSeedSC, ID_CurBuf,           curId);
                cmd.SetComputeTextureParam(s_SdfCS, s_KernelInitSeedSC, ID_SingleChannelSrc, cameraColorRT);
                cmd.DispatchCompute(s_SdfCS, s_KernelInitSeedSC, tgX, tgY, 1);
            }
            else
            {
                cmd.SetComputeTextureParam(s_SdfCS, s_KernelInitSeed, ID_CurBuf,    curId);
                cmd.SetComputeTextureParam(s_SdfCS, s_KernelInitSeed, ID_SourceTex, cameraColorRT);
                cmd.DispatchCompute(s_SdfCS, s_KernelInitSeed, tgX, tgY, 1);
            }
            cmd.ReleaseTemporaryRT(TempID_CameraColor);
        }

        // ── Jump Flooding ───────────────────────────────────────────────
        cmd.GetTemporaryRT(TempID_Previous, descARGB);
        var prevId = new RenderTargetIdentifier(TempID_Previous);

        int maxDim     = Mathf.Max(iterW, iterH);
        int iterCount  = Mathf.CeilToInt(Mathf.Log(maxDim, 2)) + 1;
        for (int i = 0; i < iterCount; i++)
        {
            cmd.Blit(curId, prevId);
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelJFA, ID_PrevBuf,   prevId);
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelJFA, ID_CurBuf,    curId);
            cmd.SetComputeIntParam    (s_SdfCS, ID_Width,     iterW);
            cmd.SetComputeIntParam    (s_SdfCS, ID_Height,    iterH);
            cmd.SetComputeIntParam    (s_SdfCS, ID_IterTime,  iterCount);
            cmd.SetComputeIntParam    (s_SdfCS, ID_Iter,      i);
            cmd.DispatchCompute(s_SdfCS, s_KernelJFA, tgX, tgY, 1);
        }
        cmd.ReleaseTemporaryRT(TempID_Previous);

        // ── GetNearest (optional) ───────────────────────────────────────
        tgX = Mathf.CeilToInt(w / 8.0f);
        tgY = Mathf.CeilToInt(h / 8.0f);

        bool useNearest = t.nearestPointSearchRange > 0;
        RenderTargetIdentifier nearestId;
        if (useNearest)
        {
            cmd.GetTemporaryRT(TempID_Nearest, descARGB);
            nearestId = new RenderTargetIdentifier(TempID_Nearest);

            cmd.SetComputeIntParam    (s_SdfCS, ID_ExtendPixels,  ext);
            cmd.SetComputeIntParam    (s_SdfCS, ID_NearestRange,  t.nearestPointSearchRange);
            cmd.SetComputeIntParam    (s_SdfCS, ID_Width,         w);
            cmd.SetComputeIntParam    (s_SdfCS, ID_Height,        h);
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelGetNearest, ID_PrevBuf, curId);
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelGetNearest, ID_CurBuf,  nearestId);
            cmd.DispatchCompute(s_SdfCS, s_KernelGetNearest, tgX, tgY, 1);
        }
        else
        {
            nearestId = curId;
        }

        // ── CalculateDistance ───────────────────────────────────────────
        float maxDist = Mathf.Sqrt(w * w + h * h);
        cmd.SetComputeIntParam  (s_SdfCS, ID_BoundaryDist,  t.boundaryDistance ? 1 : 0);
        cmd.SetComputeIntParam  (s_SdfCS, ID_Normalize,     t.normalize        ? 1 : 0);
        cmd.SetComputeFloatParam(s_SdfCS, ID_MaxDistance,   maxDist);
        cmd.SetComputeIntParam  (s_SdfCS, ID_ExtendPixels,  ext);
        cmd.SetComputeIntParam  (s_SdfCS, ID_AccurateDist,  t.accurateDistance ? 1 : 0);
        cmd.SetComputeIntParam  (s_SdfCS, ID_Width,         iterW);
        cmd.SetComputeIntParam  (s_SdfCS, ID_Height,        iterH);

        if (singleOut)
        {
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelCalcDistSC, ID_PrevBuf,  nearestId);
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelCalcDistSC, ID_CurBufSC, resultRT);
            cmd.DispatchCompute(s_SdfCS, s_KernelCalcDistSC, tgX, tgY, 1);
        }
        else
        {
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelCalcDist, ID_PrevBuf, nearestId);
            cmd.SetComputeTextureParam(s_SdfCS, s_KernelCalcDist, ID_CurBuf,  resultRT);
            cmd.DispatchCompute(s_SdfCS, s_KernelCalcDist, tgX, tgY, 1);
        }

        if (useNearest)
            cmd.ReleaseTemporaryRT(TempID_Nearest);

        cmd.ReleaseTemporaryRT(TempID_Current);

        if (t.shaderPropertyID != -1)
            cmd.SetGlobalTexture(t.shaderPropertyID, resultRT);
    }
}
