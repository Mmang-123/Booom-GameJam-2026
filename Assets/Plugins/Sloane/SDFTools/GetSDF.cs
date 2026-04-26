using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sloane
{
    public static class SDFTools
    {
        private static ComputeShader sdfComputeShader;
        private static readonly int PropertySourceTexture = Shader.PropertyToID("_SourceTexture");
        private static readonly int PropertySingleChannelSourceTexture = Shader.PropertyToID("_SingleChannelSourceTexture");
        private static readonly int PropertyPreviousBuffer = Shader.PropertyToID("_PreviousBuffer");
        private static readonly int PropertyPreviousBufferSingle = Shader.PropertyToID("_PreviousBufferSingle");
        private static readonly int PropertyCurrentBuffer = Shader.PropertyToID("_CurrentBuffer");
        private static readonly int PropertyCurrentBufferSingle = Shader.PropertyToID("_CurrentBufferSingle");
        private static readonly int PropertyMinMaxBuffer = Shader.PropertyToID("_MinMaxBuffer");
        private static readonly int PropertyWidth = Shader.PropertyToID("_Width");
        private static readonly int PropertyHeight = Shader.PropertyToID("_Height");
        private static readonly int PropertyIterationTime = Shader.PropertyToID("_IterationTime");
        private static readonly int PropertyIteration = Shader.PropertyToID("_Iteration");
        private static readonly int PropertyNearestPointSearchRange = Shader.PropertyToID("_NearestPointSearchRange");
        private static readonly int PropertyAlphaThreshold = Shader.PropertyToID("_AlphaThreshold");
        private static readonly int PropertyInvertSelection = Shader.PropertyToID("_InvertSelection");
        private static readonly int PropertyMaxDistance = Shader.PropertyToID("_MaxDistance");
        private static readonly int PropertyIsNormalized = Shader.PropertyToID("_IsNormalized");
        private static int kernelInitializeSeedSingleChannel;  
        private static int kernelInitializeSeed;
        private static int kernelJumpFlooding;
        private static int kernelGetNearest;
        private static int kernelCalculateDistance;
        private static int kernelCalculateDistanceSingleChannel;
        private static int kernelNormalizeDistance;
        private static int kernelNormalizeDistanceSingleChannel;
        private static int kernelNormalizeDistanceToSingleChannel;
        private static int kernelFindMinMaxDistance;
        private static bool initialized = false;

        private static void Initialize()
        {
            if (initialized) return;

            sdfComputeShader = Resources.Load<ComputeShader>("GetSDF");
            if (sdfComputeShader == null)
            {
                Debug.LogError("[SDFTools] Cannot find GetSDF.compute");
                return;
            }

            kernelInitializeSeedSingleChannel = sdfComputeShader.FindKernel("InitializeSeedSingleChannel");
            kernelInitializeSeed = sdfComputeShader.FindKernel("InitializeSeed");
            kernelJumpFlooding = sdfComputeShader.FindKernel("JumpFlooding");
            kernelGetNearest = sdfComputeShader.FindKernel("GetNearest");
            kernelCalculateDistance = sdfComputeShader.FindKernel("CalculateDistance");
            kernelCalculateDistanceSingleChannel = sdfComputeShader.FindKernel("CalculateDistanceSingleChannel");
            kernelNormalizeDistance = sdfComputeShader.FindKernel("NormalizeDistance");
            kernelNormalizeDistanceSingleChannel = sdfComputeShader.FindKernel("NormalizeDistanceSingleChannel");
            kernelNormalizeDistanceToSingleChannel = sdfComputeShader.FindKernel("NormalizeDistanceToSingleChannel");
            kernelFindMinMaxDistance = sdfComputeShader.FindKernel("FindMinMaxDistance");
            initialized = true;
        }

        /// <summary>
        /// 计算每个像素的最近种子点坐标
        /// </summary>
        /// <param name="sourceTexture">输入纹理</param>
        /// <param name="alphaThreshold">alpha阈值，大于此值认为有内容</param>
        /// <param name="invertSelection">是否反向选择（true=计算内部SDF，false=计算外部SDF）</param>
        /// <param name="nearestPointSearchRange">根据jfa的结果向外回字形遍历寻找精确最近点的最大距离(开销较大，运行时的功能最好别设置)</param>
        /// <returns>最近点纹理 (RG: 最近点坐标, B: 是否为种子点, A: 原始alpha)</returns>
        public static RenderTexture ComputeNearestPoint(Texture sourceTexture, float alphaThreshold = 0.5f, bool invertSelection = false, int nearestPointSearchRange = 0, bool useSingleChannel = false)
        {
            Initialize();
            if (!initialized) return null;

            int width = sourceTexture.width;
            int height = sourceTexture.height;

            // 创建临时纹理
            RenderTexture currentBuffer = CreateRenderTexture(width, height);
            RenderTexture previousBuffer = CreateRenderTexture(width, height);

            // 初始化种子缓冲区 (使用 Compute Shader)
            InitializeSeedBuffer(sourceTexture, currentBuffer, alphaThreshold, width, height, invertSelection, useSingleChannel);

            // JFA 迭代次数 (log2(max(width, height)) 向上取整)
            int maxDimension = Mathf.Max(width, height);
            int iterationCount = Mathf.CeilToInt(Mathf.Log(maxDimension, 2)) + 1;

            // 执行 Jump Flooding Algorithm
            for (int i = 0; i < iterationCount; i++)
            {
                // 交换缓冲区
                Graphics.Blit(currentBuffer, previousBuffer);

                // 设置 Compute Shader 参数
                sdfComputeShader.SetTexture(kernelJumpFlooding, PropertyPreviousBuffer, previousBuffer);
                sdfComputeShader.SetTexture(kernelJumpFlooding, PropertyCurrentBuffer, currentBuffer);
                sdfComputeShader.SetInt(PropertyWidth, width);
                sdfComputeShader.SetInt(PropertyHeight, height);
                sdfComputeShader.SetInt(PropertyIterationTime, iterationCount);
                sdfComputeShader.SetInt(PropertyIteration, i);

                int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
                int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
                sdfComputeShader.Dispatch(kernelJumpFlooding, threadGroupsX, threadGroupsY, 1);
            }

            RenderTexture.ReleaseTemporary(previousBuffer);
            RenderTexture finalBuffer;

            // 使用 GetNearest 内核精确查找最近点
            if (nearestPointSearchRange > 0)
            {
                finalBuffer = CreateRenderTexture(width, height);
                sdfComputeShader.SetTexture(kernelGetNearest, PropertyPreviousBuffer, currentBuffer);
                sdfComputeShader.SetTexture(kernelGetNearest, PropertyCurrentBuffer, finalBuffer);
                sdfComputeShader.SetInt(PropertyWidth, width);
                sdfComputeShader.SetInt(PropertyHeight, height);
                sdfComputeShader.SetInt(PropertyNearestPointSearchRange, nearestPointSearchRange);

                int threadGroupsX_final = Mathf.CeilToInt(width / 8.0f);
                int threadGroupsY_final = Mathf.CeilToInt(height / 8.0f);
                sdfComputeShader.Dispatch(kernelGetNearest, threadGroupsX_final, threadGroupsY_final, 1);

                RenderTexture.ReleaseTemporary(currentBuffer);
            }
            else
            {
                finalBuffer = currentBuffer;
            }

            return finalBuffer;
        }

        /// <summary>
        /// 获取初始化后的种子缓冲区 (用于调试)
        /// </summary>
        public static RenderTexture GetInitializedSeedBuffer(Texture sourceTexture, float alphaThreshold = 0.5f, bool invertSelection = false, bool useSingleChannel = false)
        {
            Initialize();
            if (!initialized) return null;

            int width = sourceTexture.width;
            int height = sourceTexture.height;
            RenderTexture buffer = CreateRenderTexture(width, height);
            InitializeSeedBuffer(sourceTexture, buffer, alphaThreshold, width, height, invertSelection, useSingleChannel);
            return buffer;
        }

        /// <summary>
        /// 初始化种子缓冲区：将 alpha > threshold 的像素设为种子点
        /// </summary>
        private static void InitializeSeedBuffer(Texture source, RenderTexture target, float threshold, int width, int height, bool invertSelection, bool useSingleChannel = false)
        {
            sdfComputeShader.SetTexture(kernelInitializeSeed, PropertyCurrentBuffer, target);
            sdfComputeShader.SetInt(PropertyWidth, width);
            sdfComputeShader.SetInt(PropertyHeight, height);
            sdfComputeShader.SetFloat(PropertyAlphaThreshold, threshold);
            sdfComputeShader.SetBool(PropertyInvertSelection, invertSelection);

            int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(height / 8.0f);

            if (useSingleChannel)
            {
                // 如果输入是单通道纹理，使用专门的内核
                sdfComputeShader.SetTexture(kernelInitializeSeedSingleChannel, PropertySingleChannelSourceTexture, source);
                sdfComputeShader.Dispatch(kernelInitializeSeedSingleChannel, threadGroupsX, threadGroupsY, 1);
            }
            else
            {
                // 默认使用RGBA输入
                sdfComputeShader.SetTexture(kernelInitializeSeed, PropertySourceTexture, source);
                sdfComputeShader.Dispatch(kernelInitializeSeed, threadGroupsX, threadGroupsY, 1);
            }
        }

        private static RenderTexture CreateRenderTexture(int width, int height)
        {
            var desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGBFloat, 0);
            desc.enableRandomWrite = true;
            return RenderTexture.GetTemporary(desc);
        }

        private static RenderTexture CreateRenderTexture(int width, int height, RenderTextureFormat format)
        {
            var desc = new RenderTextureDescriptor(width, height, format, 0);
            desc.enableRandomWrite = true;
            return RenderTexture.GetTemporary(desc);
        }

        private static bool IsSingleChannelFormat(RenderTextureFormat format)
        {
            return format == RenderTextureFormat.RFloat
                || format == RenderTextureFormat.RHalf
                || format == RenderTextureFormat.R8
                || format == RenderTextureFormat.R16
                || format == RenderTextureFormat.RInt;
        }

        private static void DispatchDistanceKernel(RenderTexture sourceRT, RenderTexture targetRT, int width, int height, bool useSingleChannel)
        {
            int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(height / 8.0f);

            if (useSingleChannel)
            {
                sdfComputeShader.SetTexture(kernelCalculateDistanceSingleChannel, PropertyPreviousBuffer, sourceRT);
                sdfComputeShader.SetTexture(kernelCalculateDistanceSingleChannel, PropertyCurrentBufferSingle, targetRT);
                sdfComputeShader.SetInt(PropertyWidth, width);
                sdfComputeShader.SetInt(PropertyHeight, height);
                sdfComputeShader.Dispatch(kernelCalculateDistanceSingleChannel, threadGroupsX, threadGroupsY, 1);
            }
            else
            {
                sdfComputeShader.SetTexture(kernelCalculateDistance, PropertyPreviousBuffer, sourceRT);
                sdfComputeShader.SetTexture(kernelCalculateDistance, PropertyCurrentBuffer, targetRT);
                sdfComputeShader.SetInt(PropertyWidth, width);
                sdfComputeShader.SetInt(PropertyHeight, height);
                sdfComputeShader.Dispatch(kernelCalculateDistance, threadGroupsX, threadGroupsY, 1);
            }
        }

        private static void DispatchNormalizeKernel(RenderTexture sourceRT, RenderTexture targetRT, int width, int height, float maxDistance, bool useSingleChannel)
        {
            int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(height / 8.0f);

            if (useSingleChannel)
            {
                bool sourceIsSingleChannel = IsSingleChannelFormat(sourceRT.format);
                int normalizeKernel = sourceIsSingleChannel ? kernelNormalizeDistanceSingleChannel : kernelNormalizeDistanceToSingleChannel;
                int sourceProperty = sourceIsSingleChannel ? PropertyPreviousBufferSingle : PropertyPreviousBuffer;

                sdfComputeShader.SetTexture(normalizeKernel, sourceProperty, sourceRT);
                sdfComputeShader.SetTexture(normalizeKernel, PropertyCurrentBufferSingle, targetRT);
                sdfComputeShader.SetFloat(PropertyMaxDistance, maxDistance);
                sdfComputeShader.Dispatch(normalizeKernel, threadGroupsX, threadGroupsY, 1);
            }
            else
            {
                sdfComputeShader.SetTexture(kernelNormalizeDistance, PropertyPreviousBuffer, sourceRT);
                sdfComputeShader.SetTexture(kernelNormalizeDistance, PropertyCurrentBuffer, targetRT);
                sdfComputeShader.SetFloat(PropertyMaxDistance, maxDistance);
                sdfComputeShader.Dispatch(kernelNormalizeDistance, threadGroupsX, threadGroupsY, 1);
            }
        }

        /// <summary>
        /// 将 SDF RenderTexture 转换为可读的 Texture2D
        /// </summary>
        /// <param name="sdfRT">输入的 SDF RenderTexture</param>
        /// <param name="useSingleChannelOutput">是否输出为单通道纹理（RFloat）</param>
        public static Texture2D ConvertToTexture2D(RenderTexture sdfRT, bool useSingleChannelOutput = false)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = sdfRT;

            TextureFormat outputFormat = useSingleChannelOutput ? TextureFormat.RFloat : TextureFormat.RGBAFloat;
            Texture2D tex = new Texture2D(sdfRT.width, sdfRT.height, outputFormat, false);
            tex.ReadPixels(new Rect(0, 0, sdfRT.width, sdfRT.height), 0, 0);
            tex.Apply();

            RenderTexture.active = previous;
            return tex;
        }

        /// <summary>
        /// 从 SDF 数据计算实际距离场 (GPU 版本)
        /// </summary>
        /// <param name="sourceRT">SDF 数据</param>
        /// <param name="resultRT">用于存储距离场的 RenderTexture</param>
        /// <param name="normalize">是否归一化到 [0,1] 范围</param>
        /// <returns>距离场纹理</returns>
        public static RenderTexture CalculateDistanceField(RenderTexture sourceRT, RenderTexture resultRT, bool normalize = true, bool useSingleChannelOutput = false)
        {
            Initialize();
            if (!initialized) return null;

            int width = sourceRT.width;
            int height = sourceRT.height;
            bool singleChannelOutput = useSingleChannelOutput || IsSingleChannelFormat(resultRT.format);

            // 计算原始距离
            if (!normalize)
            {
                DispatchDistanceKernel(sourceRT, resultRT, width, height, singleChannelOutput);
                return resultRT;
            }

            // 如果需要归一化
            float maxDistance = Mathf.Sqrt(width * width + height * height);
            RenderTexture rawDistanceRT = CreateRenderTexture(width, height, singleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat);
            DispatchDistanceKernel(sourceRT, rawDistanceRT, width, height, singleChannelOutput);
            DispatchNormalizeKernel(rawDistanceRT, resultRT, width, height, maxDistance, singleChannelOutput);
            RenderTexture.ReleaseTemporary(rawDistanceRT);
            return resultRT;
        }

        /// <summary>
        /// 从 SDF 数据计算实际距离场 (GPU 版本)
        /// </summary>
        /// <param name="sdfData">SDF 数据</param>
        /// <param name="normalize">是否归一化到 [0,1] 范围</param>
        /// <returns>距离场纹理</returns>
        public static RenderTexture CalculateDistanceField(RenderTexture sdfData, bool normalize = true, bool useSingleChannelOutput = false)
        {
            RenderTextureFormat outputFormat = useSingleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat;
            RenderTexture distanceRT = CreateRenderTexture(sdfData.width, sdfData.height, outputFormat);
            return CalculateDistanceField(sdfData, distanceRT, normalize, useSingleChannelOutput);
        }

        /// <summary>
        /// 根据指定的最大距离归一化距离场
        /// </summary>
        /// <param name="rawDistanceRT">未归一化的原始距离场</param>
        /// <param name="maxDistance">用于归一化的最大距离值</param>
        /// <returns>归一化后的距离场</returns>
        public static RenderTexture NormalizeDistanceField(RenderTexture rawDistanceRT, float maxDistance, bool useSingleChannelOutput = false)
        {
            Initialize();
            if (!initialized) return null;

            int width = rawDistanceRT.width;
            int height = rawDistanceRT.height;
            bool singleChannelOutput = useSingleChannelOutput || IsSingleChannelFormat(rawDistanceRT.format);

            RenderTexture normalizedRT = CreateRenderTexture(width, height, singleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat);
            DispatchNormalizeKernel(rawDistanceRT, normalizedRT, width, height, maxDistance, singleChannelOutput);

            return normalizedRT;
        }

        /// <summary>
        /// 从 SDF 数据计算实际距离场并转换为 Texture2D
        /// </summary>
        /// <param name="sourceRT">SDF 数据</param>
        /// <param name="normalize">是否归一化到 [0,1] 范围</param>
        /// <param name="useSingleChannelOutput">是否输出为单通道纹理（RFloat）</param>
        public static Texture2D CalculateDistanceFieldTexture(RenderTexture sourceRT, bool normalize = true, bool useSingleChannelOutput = false)
        {
            RenderTexture distanceRT = CalculateDistanceField(sourceRT, normalize, useSingleChannelOutput);
            Texture2D result = ConvertToTexture2D(distanceRT, useSingleChannelOutput);
            RenderTexture.ReleaseTemporary(distanceRT);
            return result;
        }

        /// <summary>
        /// 从纹理生成完整的 SDF
        /// </summary>
        /// <param name="sourceTexture">输入纹理</param>
        /// <param name="resultRT">用于存储距离场的 RenderTexture</param>
        /// <param name="alphaThreshold">alpha阈值，大于此值认为有内容</param>
        /// <param name="normalize">是否归一化距离到 [0,1] 范围</param>
        /// <param name="invertSelection">是否反向选择（true=计算内部SDF，false=计算外部SDF）</param>
        /// <returns>距离场纹理</returns>
        public static void GenerateSDF(Texture sourceTexture, RenderTexture resultRT, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, bool useSingleChannel = false)
        {
            Initialize();
            if (!initialized) return;

            // 计算最近点
            RenderTexture nearestPointRT = ComputeNearestPoint(sourceTexture, alphaThreshold, invertSelection, nearestPointSearchRange, useSingleChannel);

            // 计算距离场，输出格式跟随 resultRT
            bool useSingleChannelOutput = IsSingleChannelFormat(resultRT.format);
            CalculateDistanceField(nearestPointRT, resultRT, normalize, useSingleChannelOutput);

            // 释放中间结果
            RenderTexture.ReleaseTemporary(nearestPointRT);
        }

        public static RenderTexture GenerateSDF(Texture sourceTexture, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, bool useSingleChannel = false, bool useSingleChannelOutput = false)
        {
            RenderTextureFormat outputFormat = useSingleChannelOutput ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat;
            RenderTexture resultRT = CreateRenderTexture(sourceTexture.width, sourceTexture.height, outputFormat);
            GenerateSDF(sourceTexture, resultRT, alphaThreshold, normalize, invertSelection, nearestPointSearchRange, useSingleChannel);
            return resultRT;
        }

        /// <summary>
        /// 从纹理生成完整的 SDF 并转换为 Texture2D
        /// </summary>
        /// <param name="sourceTexture">输入纹理</param>
        /// <param name="alphaThreshold">alpha阈值</param>
        /// <param name="normalize">是否归一化到 [0,1] 范围</param>
        /// <param name="invertSelection">是否反向选择</param>
        /// <param name="nearestPointSearchRange">最近点精查范围</param>
        /// <param name="useSingleChannelOutput">是否输出为单通道纹理（RFloat）</param>
        public static Texture2D GenerateSDFTexture(Texture2D sourceTexture, float alphaThreshold = 0.5f, bool normalize = true, bool invertSelection = false, int nearestPointSearchRange = 0, bool useSingleChannelOutput = false)
        {
            RenderTexture sdfRT = GenerateSDF(sourceTexture, alphaThreshold, normalize, invertSelection, nearestPointSearchRange, useSingleChannel: false, useSingleChannelOutput: useSingleChannelOutput);
            Texture2D result = ConvertToTexture2D(sdfRT, useSingleChannelOutput);
            RenderTexture.ReleaseTemporary(sdfRT);
            return result;
        }

        /// <summary>
        /// 从SDF纹理中获取最大距离绝对值
        /// </summary>
        /// <param name="sdfRT">SDF RenderTexture</param>
        /// <param name="isNormalized">输入纹理是否已归一化</param>
        /// <returns>最大距离绝对值</returns>
        public static float GetMaxDistance(RenderTexture sdfRT, bool isNormalized)
        {
            Initialize();
            if (!initialized) return 0;

            int width = sdfRT.width;
            int height = sdfRT.height;

            // 创建StructuredBuffer：[0]=max，使用int类型
            ComputeBuffer minMaxBuffer = new ComputeBuffer(1, sizeof(int));

            // 初始化为0
            int[] initData = new int[] { 0 };
            minMaxBuffer.SetData(initData);

            // 设置Shader参数
            sdfComputeShader.SetTexture(kernelFindMinMaxDistance, PropertyPreviousBuffer, sdfRT);
            sdfComputeShader.SetBuffer(kernelFindMinMaxDistance, PropertyMinMaxBuffer, minMaxBuffer);
            sdfComputeShader.SetInt(PropertyWidth, width);
            sdfComputeShader.SetInt(PropertyHeight, height);
            sdfComputeShader.SetBool(PropertyIsNormalized, isNormalized);

            // 如果是归一化的，需要提供反归一化所需的最大距离
            if (isNormalized)
            {
                float maxDist = Mathf.Sqrt(width * width + height * height);
                sdfComputeShader.SetFloat(PropertyMaxDistance, maxDist);
            }

            // 分派计算
            int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
            sdfComputeShader.Dispatch(kernelFindMinMaxDistance, threadGroupsX, threadGroupsY, 1);

            // 读取结果
            int[] resultData = new int[1];
            minMaxBuffer.GetData(resultData);
            minMaxBuffer.Release();

            return resultData[0] / 1000.0f;
        }
    }
}