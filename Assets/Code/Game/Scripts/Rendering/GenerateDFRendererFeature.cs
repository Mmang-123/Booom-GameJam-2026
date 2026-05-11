using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ThreadData = GenerateDFRendererFeaturePass.DFProcessingThread;

public class GenerateDFRendererFeature : ScriptableRendererFeature
{
    /// <summary>
    /// Parameters for batch SDF generation: all chunks processed in a single set of dispatches
    /// using the Z dimension.  intermA/intermB must be pre-allocated Texture2DArray RTs with
    /// enableRandomWrite=true and the same Width/Height (= resolution + 2*extendPixels) and
    /// volumeDepth = chunkRangeX * chunkRangeY.
    /// </summary>
    public struct DFBatchParams
    {
        /// <summary>null = use camera color buffer (must be single-channel)</summary>
        public Texture sourceTexture;
        /// <summary>Tex2DArray ping buffer, [iterW, iterH, N], ARGBHalf, RW</summary>
        public RenderTexture intermA;
        /// <summary>Tex2DArray pong buffer (also reused as GetNearest output), same spec as intermA</summary>
        public RenderTexture intermB;
        /// <summary>Tex2DArray output, [resolution, resolution, N], R16_UNorm, RW</summary>
        public RenderTexture targetArray;
        public int chunkRangeX;
        public int chunkRangeY;
        /// <summary>Per-chunk resolution before extension (e.g. 256)</summary>
        public int resolution;
        public int extendPixels;
        public float alphaThreshold;
        public int nearestPointSearchRange;
    }

    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public Settings settings = new Settings();

    GenerateDFRendererFeaturePass m_Pass;
    public GenerateDFRendererFeaturePass Pass => m_Pass;

    // ---------- Feature 自身的缓存（Pass 的 source of truth）----------
    int m_NextID = 0;
    readonly Dictionary<int, ThreadData> m_Cache = new Dictionary<int, ThreadData>();

    bool m_HasBatch;
    DFBatchParams m_BatchCache;

    // ---------- 直接传结构体 ----------
    public int Pending(ThreadData thread)
    {
        int id = ++m_NextID;
        m_Cache[id] = thread;
        m_Pass?.PendingDFProcessingThread(id, thread);
        return id;
    }

    // ---------- 展开参数版本 ----------
    public int Pending(
        Texture sourceTexture,
        RenderTexture targetTexture,
        Vector2Int offset,
        float alphaThreshold          = 0.5f,
        bool  normalize               = true,
        bool  invertSelection         = false,
        int   nearestPointSearchRange = 0,
        int   extendPixels            = 0,
        bool  boundaryDistance        = false,
        bool  accurateDistance        = false,
        int   shaderPropertyID        = -1)
    {
        return Pending(new ThreadData
        {
            sourceTexture          = sourceTexture,
            targetTexture          = targetTexture,
            offset                 = offset,
            alphaThreshold         = alphaThreshold,
            normalize              = normalize,
            invertSelection        = invertSelection,
            nearestPointSearchRange = nearestPointSearchRange,
            extendPixels           = extendPixels,
            boundaryDistance       = boundaryDistance,
            accurateDistance       = accurateDistance,
            shaderPropertyID       = shaderPropertyID
        });
    }

    // ---------- 更新 ----------
    public void UpdateThread(int id, ThreadData thread)
    {
        if (!m_Cache.ContainsKey(id)) return;
        m_Cache[id] = thread;
        m_Pass?.UpdateDFProcessingThread(id, thread.sourceTexture, thread.targetTexture, thread.offset,
            thread.alphaThreshold, thread.normalize, thread.invertSelection,
            thread.nearestPointSearchRange, thread.extendPixels,
            thread.boundaryDistance, thread.accurateDistance, thread.shaderPropertyID);
    }

    public void UpdateThread(
        int   id,
        Texture sourceTexture,
        RenderTexture targetTexture,
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
        UpdateThread(id, new ThreadData
        {
            sourceTexture           = sourceTexture,
            targetTexture           = targetTexture,
            offset                  = offset,
            alphaThreshold          = alphaThreshold,
            normalize               = normalize,
            invertSelection         = invertSelection,
            nearestPointSearchRange = nearestPointSearchRange,
            extendPixels            = extendPixels,
            boundaryDistance        = boundaryDistance,
            accurateDistance        = accurateDistance,
            shaderPropertyID        = shaderPropertyID
        });
    }

    // ---------- 释放 ----------
    public void Release(int id)
    {
        m_Cache.Remove(id);
        m_Pass?.ReleaseDFProcessingThread(id);
    }

    // ---------- 批量SDF（单次dispatch处理所有chunk）----------
    public void PendingBatch(DFBatchParams batch)
    {
        m_HasBatch = true;
        m_BatchCache = batch;
        m_Pass?.SetBatchParams(batch, true);
    }

    public void ReleaseBatch()
    {
        m_HasBatch = false;
        m_Pass?.SetBatchParams(default, false);
    }

    // ---------- ScriptableRendererFeature ----------
    public override void Create()
    {
        m_Pass = new GenerateDFRendererFeaturePass
        {
            renderPassEvent = settings.passEvent
        };

        // 将缓存中的所有 thread 刷入新建的 Pass
        foreach (var kvp in m_Cache)
            m_Pass.PendingDFProcessingThread(kvp.Key, kvp.Value);

        if (m_HasBatch)
            m_Pass.SetBatchParams(m_BatchCache, true);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_Pass);
    }
}
