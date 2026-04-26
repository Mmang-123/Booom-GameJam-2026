using UnityEngine;
using UnityEngine.Rendering;

namespace Sloane
{
    /// <summary>
    /// SDFTools 的 CommandBuffer 版本，所有 GPU 操作录入单个 CommandBuffer，
    /// 在 Frame Debugger 中整个 SDF 生成过程显示为一个命名分组。
    /// </summary>
    public static class SDFToolsRuntime
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
        private static bool initialized = false;

        // CommandBuffer.GetTemporaryRT 使用的临时纹理 ID
        private static readonly int TempID_Current  = Shader.PropertyToID("_SDF_RT_Current");
        private static readonly int TempID_Previous = Shader.PropertyToID("_SDF_RT_Previous");
        private static readonly int TempID_Nearest  = Shader.PropertyToID("_SDF_RT_Nearest");
        private static readonly int TempID_RawDist  = Shader.PropertyToID("_SDF_RT_RawDist");

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
            kernelInitializeSeed              = sdfComputeShader.FindKernel("InitializeSeed");
            kernelJumpFlooding                = sdfComputeShader.FindKernel("JumpFlooding");
            kernelGetNearest                  = sdfComputeShader.FindKernel("GetNearest");
            kernelCalculateDistance           = sdfComputeShader.FindKernel("CalculateDistance");
            kernelCalculateDistanceSingleChannel = sdfComputeShader.FindKernel("CalculateDistanceSingleChannel");
            kernelNormalizeDistance           = sdfComputeShader.FindKernel("NormalizeDistance");
            kernelNormalizeDistanceSingleChannel = sdfComputeShader.FindKernel("NormalizeDistanceSingleChannel");
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

        /// <summary>
        /// 从纹理生成完整的 SDF，写入已有的 resultRT。
        /// 所有 GPU 步骤录入单个 CommandBuffer，Frame Debugger 中显示为一个分组。
        /// 输入是否单通道由源纹理格式自动判断。
        /// </summary>
        public static void GenerateSDF(Texture sourceTexture, RenderTexture resultRT, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0)
        {
            Initialize();
            if (!initialized) return;

            int width  = sourceTexture.width;
            int height = sourceTexture.height;
            bool useSingleChannel       = IsInputSingleChannel(sourceTexture);
            bool useSingleChannelOutput = IsSingleChannelFormat(resultRT.format);
            int threadGroupsX = Mathf.CeilToInt(width  / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(height / 8.0f);

            var cmd = new CommandBuffer { name = "SDFTools: GenerateSDF" };

            // ----------------------------------------------------------------
            // 申请中间纹理
            // ----------------------------------------------------------------
            var descARGB = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGBFloat, 0) { enableRandomWrite = true };
            cmd.GetTemporaryRT(TempID_Current,  descARGB);
            cmd.GetTemporaryRT(TempID_Previous, descARGB);

            // ----------------------------------------------------------------
            // InitializeSeed：将 alpha > threshold 的像素标记为种子点
            // ----------------------------------------------------------------
            cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
            cmd.SetComputeFloatParam(sdfComputeShader, PropertyAlphaThreshold, alphaThreshold);
            cmd.SetComputeIntParam(sdfComputeShader, PropertyInvertSelection, invertSelection ? 1 : 0);
            if (useSingleChannel)
            {
                cmd.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeedSingleChannel, PropertyCurrentBuffer, new RenderTargetIdentifier(TempID_Current));
                cmd.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeedSingleChannel, PropertySingleChannelSourceTexture, sourceTexture);
                cmd.DispatchCompute(sdfComputeShader, kernelInitializeSeedSingleChannel, threadGroupsX, threadGroupsY, 1);
            }
            else
            {
                cmd.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeed, PropertyCurrentBuffer, new RenderTargetIdentifier(TempID_Current));
                cmd.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeed, PropertySourceTexture, sourceTexture);
                cmd.DispatchCompute(sdfComputeShader, kernelInitializeSeed, threadGroupsX, threadGroupsY, 1);
            }

            // ----------------------------------------------------------------
            // Jump Flooding Algorithm
            // ----------------------------------------------------------------
            int maxDimension  = Mathf.Max(width, height);
            int iterationCount = Mathf.CeilToInt(Mathf.Log(maxDimension, 2)) + 1;
            for (int i = 0; i < iterationCount; i++)
            {
                cmd.Blit(new RenderTargetIdentifier(TempID_Current), new RenderTargetIdentifier(TempID_Previous));
                cmd.SetComputeTextureParam(sdfComputeShader, kernelJumpFlooding, PropertyPreviousBuffer, new RenderTargetIdentifier(TempID_Previous));
                cmd.SetComputeTextureParam(sdfComputeShader, kernelJumpFlooding, PropertyCurrentBuffer,  new RenderTargetIdentifier(TempID_Current));
                cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth,         width);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight,        height);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyIterationTime, iterationCount);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyIteration,     i);
                cmd.DispatchCompute(sdfComputeShader, kernelJumpFlooding, threadGroupsX, threadGroupsY, 1);
            }
            cmd.ReleaseTemporaryRT(TempID_Previous);

            // ----------------------------------------------------------------
            // GetNearest（可选精查）
            // ----------------------------------------------------------------
            int nearestId;
            if (nearestPointSearchRange > 0)
            {
                cmd.GetTemporaryRT(TempID_Nearest, descARGB);
                cmd.SetComputeTextureParam(sdfComputeShader, kernelGetNearest, PropertyPreviousBuffer, new RenderTargetIdentifier(TempID_Current));
                cmd.SetComputeTextureParam(sdfComputeShader, kernelGetNearest, PropertyCurrentBuffer,  new RenderTargetIdentifier(TempID_Nearest));
                cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth,                    width);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight,                   height);
                cmd.SetComputeIntParam(sdfComputeShader, PropertyNearestPointSearchRange,  nearestPointSearchRange);
                cmd.DispatchCompute(sdfComputeShader, kernelGetNearest, threadGroupsX, threadGroupsY, 1);
                cmd.ReleaseTemporaryRT(TempID_Current);
                nearestId = TempID_Nearest;
            }
            else
            {
                nearestId = TempID_Current;
            }

            // ----------------------------------------------------------------
            // CalculateDistance（+ 可选 Normalize）
            // ----------------------------------------------------------------
            if (!normalize)
            {
                if (useSingleChannelOutput)
                {
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyPreviousBuffer,    new RenderTargetIdentifier(nearestId));
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyCurrentBufferSingle, resultRT);
                    cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth,  width);
                    cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                    cmd.DispatchCompute(sdfComputeShader, kernelCalculateDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);
                }
                else
                {
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyPreviousBuffer, new RenderTargetIdentifier(nearestId));
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyCurrentBuffer,  resultRT);
                    cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth,  width);
                    cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                    cmd.DispatchCompute(sdfComputeShader, kernelCalculateDistance, threadGroupsX, threadGroupsY, 1);
                }
            }
            else
            {
                float maxDist  = Mathf.Sqrt(width * width + height * height);
                var   rawFormat = useSingleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat;
                var   descRaw   = new RenderTextureDescriptor(width, height, rawFormat, 0) { enableRandomWrite = true };
                cmd.GetTemporaryRT(TempID_RawDist, descRaw);

                if (useSingleChannelOutput)
                {
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyPreviousBuffer,     new RenderTargetIdentifier(nearestId));
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyCurrentBufferSingle, new RenderTargetIdentifier(TempID_RawDist));
                    cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth,  width);
                    cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                    cmd.DispatchCompute(sdfComputeShader, kernelCalculateDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);

                    cmd.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistanceSingleChannel, PropertyPreviousBufferSingle, new RenderTargetIdentifier(TempID_RawDist));
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistanceSingleChannel, PropertyCurrentBufferSingle,  resultRT);
                    cmd.SetComputeFloatParam(sdfComputeShader, PropertyMaxDistance, maxDist);
                    cmd.DispatchCompute(sdfComputeShader, kernelNormalizeDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);
                }
                else
                {
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyPreviousBuffer, new RenderTargetIdentifier(nearestId));
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyCurrentBuffer,  new RenderTargetIdentifier(TempID_RawDist));
                    cmd.SetComputeIntParam(sdfComputeShader, PropertyWidth,  width);
                    cmd.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                    cmd.DispatchCompute(sdfComputeShader, kernelCalculateDistance, threadGroupsX, threadGroupsY, 1);

                    cmd.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistance, PropertyPreviousBuffer, new RenderTargetIdentifier(TempID_RawDist));
                    cmd.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistance, PropertyCurrentBuffer,  resultRT);
                    cmd.SetComputeFloatParam(sdfComputeShader, PropertyMaxDistance, maxDist);
                    cmd.DispatchCompute(sdfComputeShader, kernelNormalizeDistance, threadGroupsX, threadGroupsY, 1);
                }

                cmd.ReleaseTemporaryRT(TempID_RawDist);
            }

            cmd.ReleaseTemporaryRT(nearestId);

            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();
        }

        /// <summary>
        /// 从纹理生成完整的 SDF，返回新申请的临时 RenderTexture（调用方负责 ReleaseTemporary）。
        /// 输入是否单通道由源纹理格式自动判断；输出格式由 useSingleChannelOutput 控制。
        /// </summary>
        public static RenderTexture GenerateSDF(Texture sourceTexture, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, bool useSingleChannelOutput = false)
        {
            var outputFormat = useSingleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat;
            var desc = new RenderTextureDescriptor(sourceTexture.width, sourceTexture.height, outputFormat, 0) { enableRandomWrite = true };
            RenderTexture resultRT = RenderTexture.GetTemporary(desc);
            GenerateSDF(sourceTexture, resultRT, alphaThreshold, normalize, invertSelection, nearestPointSearchRange);
            return resultRT;
        }
    }
}
