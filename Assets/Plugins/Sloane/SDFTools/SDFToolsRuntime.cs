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
        private static int kernelFillBoundingDistance;
        private static int kernelFillBoundingDistanceSingleChannel;
        private static bool initialized = false;

        // CommandBuffer.GetTemporaryRT 使用的临时纹理 ID
        private static readonly int TempID_Previous = Shader.PropertyToID("_SDF_RT_Previous");
        private static readonly int TempID_Nearest = Shader.PropertyToID("_SDF_RT_Nearest");
        private static readonly int TempID_RawDist = Shader.PropertyToID("_SDF_RT_RawDist");
        private static readonly int PropertyHasSeedBuffer = Shader.PropertyToID("_HasSeedBuffer");
        private static readonly int PropertyBoundaryDistance = Shader.PropertyToID("_BoundaryDistance");

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

        /// <summary>
        /// 从纹理生成完整的 SDF，写入已有的 resultRT。
        /// 先运行 InitializeSeed 并回读 CPU 判断是否有种子点；
        /// 若无种子，直接将 resultRT 填充为最大距离并返回。
        /// 全程分两个 CommandBuffer 执行，各自在 Frame Debugger 中显示为独立分组。
        /// </summary>
        public static void GenerateSDF(Texture sourceTexture, RenderTexture resultRT, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, bool boundaryDistance = false)
        {
            Initialize();
            if (!initialized) return;

            int width = sourceTexture.width;
            int height = sourceTexture.height;
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

            var hasSeedBuffer = new ComputeBuffer(1, sizeof(int));
            hasSeedBuffer.SetData(new int[] { 0 });

            var cmdInit = new CommandBuffer { name = "SDFTools: GenerateSDF [1/2] InitSeed" };
            cmdInit.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
            cmdInit.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
            cmdInit.SetComputeFloatParam(sdfComputeShader, PropertyAlphaThreshold, alphaThreshold);
            cmdInit.SetComputeIntParam(sdfComputeShader, PropertyInvertSelection, invertSelection ? 1 : 0);
            if (useSingleChannel)
            {
                cmdInit.SetComputeBufferParam(sdfComputeShader, kernelInitializeSeedSingleChannel, PropertyHasSeedBuffer, hasSeedBuffer);
                cmdInit.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeedSingleChannel, PropertyCurrentBuffer, currentBuffer);
                cmdInit.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeedSingleChannel, PropertySingleChannelSourceTexture, sourceTexture);
                cmdInit.DispatchCompute(sdfComputeShader, kernelInitializeSeedSingleChannel, threadGroupsX, threadGroupsY, 1);
            }
            else
            {
                cmdInit.SetComputeBufferParam(sdfComputeShader, kernelInitializeSeed, PropertyHasSeedBuffer, hasSeedBuffer);
                cmdInit.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeed, PropertyCurrentBuffer, currentBuffer);
                cmdInit.SetComputeTextureParam(sdfComputeShader, kernelInitializeSeed, PropertySourceTexture, sourceTexture);
                cmdInit.DispatchCompute(sdfComputeShader, kernelInitializeSeed, threadGroupsX, threadGroupsY, 1);
            }
            Graphics.ExecuteCommandBuffer(cmdInit);
            cmdInit.Release();

            // CPU 回读 flag
            var flagData = new int[1];
            hasSeedBuffer.GetData(flagData);
            hasSeedBuffer.Release();

            if (flagData[0] == 0)
            {
                if (!boundaryDistance)
                {
                    // 全空且不需边界距离：填充最大距离并返回
                    float fillValue = normalize ? 1.0f : Mathf.Sqrt(width * width + height * height);
                    var cmdClear = new CommandBuffer { name = "SDFTools: GenerateSDF - NoContent" };
                    cmdClear.SetRenderTarget(resultRT);
                    cmdClear.ClearRenderTarget(false, true, new Color(fillValue, fillValue, fillValue, fillValue));
                    Graphics.ExecuteCommandBuffer(cmdClear);
                    cmdClear.Release();
                    RenderTexture.ReleaseTemporary(currentBuffer);
                    return;
                }
                else
                {
                    // 全空但需边界距离：直接用专用 kernel 写入每像素到边界的距离
                    var cmdFill = new CommandBuffer { name = "SDFTools: GenerateSDF - BoundaryOnly" };
                    cmdFill.SetComputeIntParam(sdfComputeShader, PropertyWidth,  width);
                    cmdFill.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);

                    if (normalize)
                    {
                        float maxDist = Mathf.Sqrt(width * width + height * height);
                        var rawFormat = useSingleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat;
                        var descRaw = new RenderTextureDescriptor(width, height, rawFormat, 0) { enableRandomWrite = true };
                        cmdFill.GetTemporaryRT(TempID_RawDist, descRaw);

                        if (useSingleChannelOutput)
                        {
                            cmdFill.SetComputeTextureParam(sdfComputeShader, kernelFillBoundingDistanceSingleChannel, PropertyCurrentBufferSingle, new RenderTargetIdentifier(TempID_RawDist));
                            cmdFill.DispatchCompute(sdfComputeShader, kernelFillBoundingDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);
                            cmdFill.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistanceSingleChannel, PropertyPreviousBufferSingle, new RenderTargetIdentifier(TempID_RawDist));
                            cmdFill.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistanceSingleChannel, PropertyCurrentBufferSingle, resultRT);
                            cmdFill.SetComputeFloatParam(sdfComputeShader, PropertyMaxDistance, maxDist);
                            cmdFill.DispatchCompute(sdfComputeShader, kernelNormalizeDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);
                        }
                        else
                        {
                            cmdFill.SetComputeTextureParam(sdfComputeShader, kernelFillBoundingDistance, PropertyCurrentBuffer, new RenderTargetIdentifier(TempID_RawDist));
                            cmdFill.DispatchCompute(sdfComputeShader, kernelFillBoundingDistance, threadGroupsX, threadGroupsY, 1);
                            cmdFill.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistance, PropertyPreviousBuffer, new RenderTargetIdentifier(TempID_RawDist));
                            cmdFill.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistance, PropertyCurrentBuffer, resultRT);
                            cmdFill.SetComputeFloatParam(sdfComputeShader, PropertyMaxDistance, maxDist);
                            cmdFill.DispatchCompute(sdfComputeShader, kernelNormalizeDistance, threadGroupsX, threadGroupsY, 1);
                        }

                        cmdFill.ReleaseTemporaryRT(TempID_RawDist);
                    }
                    else
                    {
                        if (useSingleChannelOutput)
                        {
                            cmdFill.SetComputeTextureParam(sdfComputeShader, kernelFillBoundingDistanceSingleChannel, PropertyCurrentBufferSingle, resultRT);
                            cmdFill.DispatchCompute(sdfComputeShader, kernelFillBoundingDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);
                        }
                        else
                        {
                            cmdFill.SetComputeTextureParam(sdfComputeShader, kernelFillBoundingDistance, PropertyCurrentBuffer, resultRT);
                            cmdFill.DispatchCompute(sdfComputeShader, kernelFillBoundingDistance, threadGroupsX, threadGroupsY, 1);
                        }
                    }

                    Graphics.ExecuteCommandBuffer(cmdFill);
                    cmdFill.Release();
                    RenderTexture.ReleaseTemporary(currentBuffer);
                    return;
                }
            }

            // ----------------------------------------------------------------
            // 阶段二：JFA + GetNearest + Distance + Normalize
            // ----------------------------------------------------------------
            var cmdCompute = new CommandBuffer { name = "SDFTools: GenerateSDF [2/2] Compute" };
            cmdCompute.GetTemporaryRT(TempID_Previous, descARGB);

            // JFA 迭代
            int maxDimension = Mathf.Max(width, height);
            int iterationCount = Mathf.CeilToInt(Mathf.Log(maxDimension, 2)) + 1;
            for (int i = 0; i < iterationCount; i++)
            {
                // cmdCompute.Blit(currentBuffer, new RenderTargetIdentifier(TempID_Previous));
                cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelJumpFlooding, PropertyPreviousBuffer, new RenderTargetIdentifier(TempID_Previous));
                cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelJumpFlooding, PropertyCurrentBuffer, currentBuffer);
                cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyIterationTime, iterationCount);
                cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyIteration, i);
                cmdCompute.DispatchCompute(sdfComputeShader, kernelJumpFlooding, threadGroupsX, threadGroupsY, 1);
            }
            cmdCompute.ReleaseTemporaryRT(TempID_Previous);

            // GetNearest（可选精查）
            RenderTexture nearestBuffer;
            if (nearestPointSearchRange > 0)
            {
                cmdCompute.GetTemporaryRT(TempID_Nearest, descARGB);
                cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelGetNearest, PropertyPreviousBuffer, currentBuffer);
                cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelGetNearest, PropertyCurrentBuffer, new RenderTargetIdentifier(TempID_Nearest));
                cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyNearestPointSearchRange, nearestPointSearchRange);
                cmdCompute.DispatchCompute(sdfComputeShader, kernelGetNearest, threadGroupsX, threadGroupsY, 1);
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

            // 传入 _BoundaryDistance flag
            cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyBoundaryDistance, boundaryDistance ? 1 : 0);

            if (!normalize)
            {
                if (useSingleChannelOutput)
                {
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyPreviousBuffer, nearestId);
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyCurrentBufferSingle, resultRT);
                    cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                    cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                    cmdCompute.DispatchCompute(sdfComputeShader, kernelCalculateDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);
                }
                else
                {
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyPreviousBuffer, nearestId);
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyCurrentBuffer, resultRT);
                    cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                    cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                    cmdCompute.DispatchCompute(sdfComputeShader, kernelCalculateDistance, threadGroupsX, threadGroupsY, 1);
                }
            }
            else
            {
                float maxDist = Mathf.Sqrt(width * width + height * height);
                var rawFormat = useSingleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat;
                var descRaw = new RenderTextureDescriptor(width, height, rawFormat, 0) { enableRandomWrite = true };
                cmdCompute.GetTemporaryRT(TempID_RawDist, descRaw);

                if (useSingleChannelOutput)
                {
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyPreviousBuffer, nearestId);
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistanceSingleChannel, PropertyCurrentBufferSingle, new RenderTargetIdentifier(TempID_RawDist));
                    cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                    cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                    cmdCompute.DispatchCompute(sdfComputeShader, kernelCalculateDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);

                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistanceSingleChannel, PropertyPreviousBufferSingle, new RenderTargetIdentifier(TempID_RawDist));
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistanceSingleChannel, PropertyCurrentBufferSingle, resultRT);
                    cmdCompute.SetComputeFloatParam(sdfComputeShader, PropertyMaxDistance, maxDist);
                    cmdCompute.DispatchCompute(sdfComputeShader, kernelNormalizeDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);
                }
                else
                {
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyPreviousBuffer, nearestId);
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelCalculateDistance, PropertyCurrentBuffer, new RenderTargetIdentifier(TempID_RawDist));
                    cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyWidth, width);
                    cmdCompute.SetComputeIntParam(sdfComputeShader, PropertyHeight, height);
                    cmdCompute.DispatchCompute(sdfComputeShader, kernelCalculateDistance, threadGroupsX, threadGroupsY, 1);

                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistance, PropertyPreviousBuffer, new RenderTargetIdentifier(TempID_RawDist));
                    cmdCompute.SetComputeTextureParam(sdfComputeShader, kernelNormalizeDistance, PropertyCurrentBuffer, resultRT);
                    cmdCompute.SetComputeFloatParam(sdfComputeShader, PropertyMaxDistance, maxDist);
                    cmdCompute.DispatchCompute(sdfComputeShader, kernelNormalizeDistance, threadGroupsX, threadGroupsY, 1);
                }

                cmdCompute.ReleaseTemporaryRT(TempID_RawDist);
            }

            if (useNearestTemp)
                cmdCompute.ReleaseTemporaryRT(TempID_Nearest);

            Graphics.ExecuteCommandBuffer(cmdCompute);
            cmdCompute.Release();

            RenderTexture.ReleaseTemporary(currentBuffer);
        }

        /// <summary>
        /// 从纹理生成完整的 SDF，返回新申请的临时 RenderTexture（调用方负责 ReleaseTemporary）。
        /// 输入是否单通道由源纹理格式自动判断；输出格式由 useSingleChannelOutput 控制。
        /// </summary>
        public static RenderTexture GenerateSDF(Texture sourceTexture, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, bool useSingleChannelOutput = false, bool boundaryDistance = false)
        {
            var outputFormat = useSingleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat;
            var desc = new RenderTextureDescriptor(sourceTexture.width, sourceTexture.height, outputFormat, 0) { enableRandomWrite = true };
            RenderTexture resultRT = RenderTexture.GetTemporary(desc);
            GenerateSDF(sourceTexture, resultRT, alphaThreshold, normalize, invertSelection, nearestPointSearchRange, boundaryDistance);
            return resultRT;
        }
    }
}
