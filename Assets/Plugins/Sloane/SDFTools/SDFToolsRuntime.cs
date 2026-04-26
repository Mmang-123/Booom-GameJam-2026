using UnityEngine;
using UnityEngine.Rendering;

namespace Sloane
{
    /// <summary>
    /// SDFTools 的 CommandBuffer 版本，所有 GPU 操作录入单个 CommandBuffer，
    /// 在 Frame Debugger 中整个 SDF 生成过程显示为一个命名分组。
    /// </summary>
    public static class SDFTools
    {
        private static ComputeShader sdfComputeShader;
        private static readonly int PropertySourceTexture = Shader.PropertyToID("_SourceTexture");
        private static readonly int PropertySingleChannelSourceTexture = Shader.PropertyToID("_SingleChannelSourceTexture");
        private static readonly int PropertyPreviousBuffer = Shader.PropertyToID("_PreviousBuffer");
        private static readonly int PropertyPreviousBufferSingle = Shader.PropertyToID("_PreviousBufferSingle");
        private static readonly int PropertyCurrentBuffer = Shader.PropertyToID("_CurrentBuffer");
        private static readonly int PropertyCurrentBufferSingle = Shader.PropertyToID("_CurrentBufferSingle");
        private static readonly int PropertyWidth = Shader.PropertyToID("_Width");
        private static readonly int PropertyHeight = Shader.PropertyToID("_Height");
        private static readonly int PropertyIterationTime = Shader.PropertyToID("_IterationTime");
        private static readonly int PropertyIteration = Shader.PropertyToID("_Iteration");
        private static readonly int PropertyNearestPointSearchRange = Shader.PropertyToID("_NearestPointSearchRange");
        private static readonly int PropertyAlphaThreshold = Shader.PropertyToID("_AlphaThreshold");
        private static readonly int PropertyInvertSelection = Shader.PropertyToID("_InvertSelection");
        private static readonly int PropertyMaxDistance = Shader.PropertyToID("_MaxDistance");
        private static int kernelInitializeSeedSingleChannel;
        private static int kernelInitializeSeed;
        private static int kernelJumpFlooding;
        private static int kernelGetNearest;
        private static int kernelCalculateDistance;
        private static int kernelCalculateDistanceSingleChannel;
        private static int kernelNormalizeDistance;
        private static int kernelNormalizeDistanceSingleChannel;
        private static int kernelFillBoundingDistance;
        private static int kernelFillBoundingDistanceSingleChannel;
        private static int kernelCombineInnerOuterSingle;
        private static int kernelPackSDFToRGB;
        private static bool initialized = false;

        // CommandBuffer.GetTemporaryRT 使用的临时纹理 ID
        private static readonly int TempID_Previous = Shader.PropertyToID("_SDF_RT_Previous");
        private static readonly int TempID_Nearest = Shader.PropertyToID("_SDF_RT_Nearest");
        private static readonly int TempID_RawDist = Shader.PropertyToID("_SDF_RT_RawDist");
        private static readonly int PropertyHasSeedBuffer = Shader.PropertyToID("_HasSeedBuffer");
        private static readonly int PropertyBoundaryDistance = Shader.PropertyToID("_BoundaryDistance");
        private static readonly int PropertyNormalize = Shader.PropertyToID("_Normalize");
        private static readonly int PropertyOffsetX = Shader.PropertyToID("_OffsetX");
        private static readonly int PropertyOffsetY = Shader.PropertyToID("_OffsetY");


        private static void Initialize()
        {
            if (initialized) return;

            sdfComputeShader = Resources.Load<ComputeShader>("GetSDF");
            if (sdfComputeShader == null)
            {
                Debug.LogError("[SDFToolsRuntime] Cannot find GetSDF.compute");
                return;
            }

            kernelInitializeSeedSingleChannel = sdfComputeShader.FindKernel("InitializeSeedSingleChannel");
            kernelInitializeSeed = sdfComputeShader.FindKernel("InitializeSeed");
            kernelJumpFlooding = sdfComputeShader.FindKernel("JumpFlooding");
            kernelGetNearest = sdfComputeShader.FindKernel("GetNearest");
            kernelCalculateDistance = sdfComputeShader.FindKernel("CalculateDistance");
            kernelCalculateDistanceSingleChannel = sdfComputeShader.FindKernel("CalculateDistanceSingleChannel");
            kernelNormalizeDistance           = sdfComputeShader.FindKernel("NormalizeDistance");
            kernelNormalizeDistanceSingleChannel = sdfComputeShader.FindKernel("NormalizeDistanceSingleChannel");
            kernelFillBoundingDistance           = sdfComputeShader.FindKernel("FillBoundingDistance");
            kernelFillBoundingDistanceSingleChannel = sdfComputeShader.FindKernel("FillBoundingDistanceSingleChannel");
            initialized = true;
        }

        private static bool IsSingleChannelFormat(RenderTextureFormat format)
        {
            return format == RenderTextureFormat.RFloat
                || format == RenderTextureFormat.RHalf
                || format == RenderTextureFormat.R8
                || format == RenderTextureFormat.R16
                || format == RenderTextureFormat.RInt;
        }

        private static bool IsInputSingleChannel(Texture tex)
        {
            if (tex is RenderTexture rt)
                return IsSingleChannelFormat(rt.format);
            if (tex is Texture2D tex2d)
                return tex2d.format == TextureFormat.RFloat
                    || tex2d.format == TextureFormat.RHalf
                    || tex2d.format == TextureFormat.R8
                    || tex2d.format == TextureFormat.R16
                    || tex2d.format == TextureFormat.Alpha8;
            return false;
        }

        public static void GenerateDF(Texture sourceTexture, RenderTexture resultRT, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, bool boundaryDistance = false)
        {
            GenerateDF(sourceTexture, resultRT, Vector2Int.zero, alphaThreshold, normalize, invertSelection, nearestPointSearchRange, boundaryDistance);
        }

        /// <summary>
        /// 从纹理生成完整的 DF，写入已有的 resultRT。
        /// 先运行 InitializeSeed 并回读 CPU 判断是否有种子点；
        /// 若无种子，直接将 resultRT 填充为最大距离并返回。
        /// 全程分两个 CommandBuffer 执行，各自在 Frame Debugger 中显示为独立分组。
        /// </summary>
        public static void GenerateDF(Texture sourceTexture, RenderTexture resultRT, Vector2Int offset, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, bool boundaryDistance = false)
        {
            Initialize();
            if (!initialized) return;

            int width = resultRT.width;
            int height = resultRT.height;
            bool useSingleChannel = IsInputSingleChannel(sourceTexture);
            bool useSingleChannelOutput = IsSingleChannelFormat(resultRT.format);
            int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
            var descARGB = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGBFloat, 0) { enableRandomWrite = true };

            // ----------------------------------------------------------------
            // 阶段一：InitializeSeed + 回读是否有种子点
            // currentBuffer 跨两个 CommandBuffer 存活，不用 cmd.GetTemporaryRT
            // ----------------------------------------------------------------
            RenderTexture currentBuffer = RenderTexture.GetTemporary(descARGB);
            currentBuffer.Create();

            var cmd = new CommandBuffer { name = "GenerateSDF" };
            cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
            cmd.SetComputeFloatParam(sdfComputeShader, PropertyAlphaThreshold, alphaThreshold);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyInvertSelection, invertSelection ? 1 : 0);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyOffsetX, offset.x);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyOffsetY, offset.y);
            if (useSingleChannel)
            {
                cmd.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeedSingleChannel, PropertyCurrentBuffer, currentBuffer);
                cmd.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeedSingleChannel, PropertySingleChannelSourceTexture, sourceTexture);
                cmd.DispatchCompute(sdfComputeShader, kernelInitializeSeedSingleChannel, threadGroupsX, threadGroupsY, 1);
            }
            else
            {
                cmd.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeed, PropertyCurrentBuffer, currentBuffer);
                cmd.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeed, PropertySourceTexture, sourceTexture);
                cmd.DispatchCompute(sdfComputeShader, kernelInitializeSeed, threadGroupsX, threadGroupsY, 1);
            }

            cmd.GetTemporaryRT(TempID_Previous, descARGB);

            // JFA 迭代
            int maxDimension = Mathf.Max(width, height);
            int iterationCount = Mathf.CeilToInt(Mathf.Log(maxDimension, 2)) + 1;
            for (int i = 0; i < iterationCount; i++)
            {
                // cmdCompute.Blit(currentBuffer, new RenderTargetIdentifier(TempID_Previous));
                cmd.SetComputeTextureParam(sdfComputeShader, kernelJumpFlooding, PropertyPreviousBuffer, new RenderTargetIdentifier(TempID_Previous));
                cmd.SetComputeTextureParam(sdfComputeShader, kernelJumpFlooding, PropertyCurrentBuffer, currentBuffer);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyIterationTime, iterationCount);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyIteration, i);
                cmd.DispatchCompute(sdfComputeShader, kernelJumpFlooding, threadGroupsX, threadGroupsY, 1);
            }
            cmd.ReleaseTemporaryRT(TempID_Previous);

            // GetNearest（可选精查）
            RenderTexture nearestBuffer;
            if (nearestPointSearchRange > 0)
            {
                cmd.GetTemporaryRT(TempID_Nearest, descARGB);
                cmd.SetComputeTextureParam(sdfComputeShader, kernelGetNearest, PropertyPreviousBuffer, currentBuffer);
                cmd.SetComputeTextureParam(sdfComputeShader, kernelGetNearest, PropertyCurrentBuffer, new RenderTargetIdentifier(TempID_Nearest));
                cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyNearestPointSearchRange, nearestPointSearchRange);
                cmd.DispatchCompute(sdfComputeShader, kernelGetNearest, threadGroupsX, threadGroupsY, 1);
                // currentBuffer 在下面不再使用，延迟到 Execute 后释放
                nearestBuffer = null; // 占位，实际使用 TempID_Nearest
            }
            else
            {
                nearestBuffer = currentBuffer;
            }

            // CalculateDistance + 可选 Normalize
            bool useNearestTemp = nearestPointSearchRange > 0;
            RenderTargetIdentifier nearestId = useNearestTemp
                ? new RenderTargetIdentifier(TempID_Nearest)
                : new RenderTargetIdentifier(nearestBuffer);

            // 传入 flag 并直接计算距离（可选内联归一化）
            float maxDist = Mathf.Sqrt(width * width + height * height);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyBoundaryDistance, boundaryDistance ? 1 : 0);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyNormalize, normalize ? 1 : 0);
            cmd.SetComputeFloatParam(sdfComputeShader, PropertyMaxDistance, maxDist);

            if (useSingleChannelOutput)
            {
                cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyPreviousBuffer, nearestId);
                cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyCurrentBufferSingle, resultRT);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                cmd.DispatchCompute(sdfComputeShader, kernelCalculateDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);
            }
            else
            {
                cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyPreviousBuffer, nearestId);
                cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyCurrentBuffer, resultRT);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                cmd.DispatchCompute(sdfComputeShader, kernelCalculateDistance, threadGroupsX, threadGroupsY, 1);
            }

            if (useNearestTemp)
                cmd.ReleaseTemporaryRT(TempID_Nearest);

            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();

            RenderTexture.ReleaseTemporary(currentBuffer);
        }

        public static void GenerateSDF(Texture sourceTexture, RenderTexture resultRT, float alphaThreshold = 0.5f, int nearestPointSearchRange = 0)
        {
            GenerateSDF(sourceTexture, Vector2Int.zero, resultRT, alphaThreshold, nearestPointSearchRange);
        }

        public static void GenerateSDF(Texture sourceTexture, Vector2Int offset, RenderTexture resultRT, float alphaThreshold = 0.5f, int nearestPointSearchRange = 0)
        {
            RenderTexture innerRT = RenderTexture.GetTemporary(resultRT.descriptor);
            innerRT.Create();
            GenerateDF(sourceTexture, innerRT, offset, alphaThreshold, false, true, nearestPointSearchRange, false);

            RenderTexture outerRT = RenderTexture.GetTemporary(resultRT.descriptor);
            outerRT.Create();
            GenerateDF(sourceTexture, outerRT, offset, alphaThreshold, false, false, nearestPointSearchRange, false);

            CommandBuffer cmd = new CommandBuffer { name = "CombineInnerOuter" };
            cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth, sourceTexture.width);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, sourceTexture.height);
            cmd.SetComputeTextureParam(sdfComputeShader, kernelCombineInnerOuterSingle, PropertyPreviousBufferSingle, outerRT);
            cmd.SetComputeTextureParam(sdfComputeShader, kernelCombineInnerOuterSingle, PropertySourceTexture, outerRT);
            cmd.SetComputeTextureParam(sdfComputeShader, kernelCombineInnerOuterSingle, PropertyCurrentBufferSingle, resultRT);

            int threadGroupsX = Mathf.CeilToInt(sourceTexture.width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(sourceTexture.height / 8.0f);
            cmd.DispatchCompute(sdfComputeShader, kernelCombineInnerOuterSingle, threadGroupsX, threadGroupsY, 1);

            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();

            RenderTexture.ReleaseTemporary(innerRT);
            RenderTexture.ReleaseTemporary(outerRT);
        }

        public static Texture2D GenerateSDF(Texture sourceTexture, float alphaThreshold = 0.5f, int nearestPointSearchRange = 0, int factor = 64)
        {
            var desc = new RenderTextureDescriptor(sourceTexture.width, sourceTexture.height, RenderTextureFormat.RFloat, 0) { enableRandomWrite = true };
            RenderTexture sdfRT = RenderTexture.GetTemporary(desc);
            
            GenerateSDF(sourceTexture, sdfRT, alphaThreshold, nearestPointSearchRange);

            CommandBuffer cmd = new CommandBuffer { name = "PackingSDF" };
            cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth, sourceTexture.width);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, sourceTexture.height);
            cmd.SetComputeTextureParam(sdfComputeShader, kernelPackSDFToRGB, PropertyPreviousBufferSingle, sdfRT);
            cmd.SetComputeTextureParam(sdfComputeShader, kernelPackSDFToRGB, PropertyCurrentBuffer, sdfRT);
            cmd.SetComputeFloatParam(sdfComputeShader, PropertyMaxDistance, factor);

            int threadGroupsX = Mathf.CeilToInt(sourceTexture.width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(sourceTexture.height / 8.0f);
            cmd.DispatchCompute(sdfComputeShader, kernelPackSDFToRGB, threadGroupsX, threadGroupsY, 1);

            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();

            Texture2D result = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = sdfRT;
            result.ReadPixels(new Rect(0, 0, sdfRT.width, sdfRT.height), 0, 0);
            result.Apply();

            RenderTexture.ReleaseTemporary(sdfRT);
            return result;
        }

        /// <summary>
        /// 从纹理生成完整的 SDF，返回新申请的临时 RenderTexture（调用方负责 ReleaseTemporary）。
        /// 输入是否单通道由源纹理格式自动判断；输出格式由 useSingleChannelOutput 控制。
        /// </summary>
        public static RenderTexture GenerateDF(Texture sourceTexture, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, bool useSingleChannelOutput = false, bool boundaryDistance = false)
        {
            var outputFormat = useSingleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat;
            var desc = new RenderTextureDescriptor(sourceTexture.width, sourceTexture.height, outputFormat, 0) { enableRandomWrite = true };
            RenderTexture resultRT = RenderTexture.GetTemporary(desc);
            GenerateDF(sourceTexture, resultRT, alphaThreshold, normalize, invertSelection, nearestPointSearchRange, boundaryDistance);
            return resultRT;
        }
    }
}
