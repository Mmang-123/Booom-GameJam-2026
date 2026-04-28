using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Mmang.PixelartRender
{

    public enum EPixelartBuffer
    {
        Depth = 0, // 原深度缓冲
        DepthNormal = 1, // 第二层深度和法线(屏幕空间), 用于描边 
        Albedo = 2,
        SmoothnessMetallic = 3,
        Emission = 4,
        OriginUV = 5, // 原点的屏幕空间UV
        Properties = 6, // R: LUT索引  G: 描边+阴影采样模式  B: 2D障碍遮罩
        SpecularOutput = 7,

        [MEnum(hide = true)] End,
    }

    public static class PRenderStage
    {
        public const EPixelartBuffer RawDataStart = EPixelartBuffer.DepthNormal;
        public const EPixelartBuffer RawDataEnd = EPixelartBuffer.Properties;
    }

    public static class PBuffer
    {
        private static List<string> s_BufferNames;
        private static List<int> s_BufferShaderProperties;
        private static bool s_Init = false;

        private static void Init()
        {
            if (s_Init)
                return;
            s_Init = true;

            s_BufferNames = new();

            s_BufferShaderProperties = new();
            for (int i = 0; i < (int)EPixelartBuffer.End; i++)
            {
                string enumName = System.Enum.GetName(typeof(EPixelartBuffer), (EPixelartBuffer)i);
                s_BufferNames.Add(enumName);
                s_BufferShaderProperties.Add(Shader.PropertyToID($"_{enumName}Buffer"));
            }
        }

        public static string GetBufferName(EPixelartBuffer bufferType)
        {
            Init();
            return s_BufferNames[(int)bufferType];
        }

        public static int GetBufferShaderProperty(EPixelartBuffer bufferType)
        {
            Init();
            return s_BufferShaderProperties[(int)bufferType];
        }

        public static RenderTextureDescriptor GetBufferDescriptor(PixelartCameraData cameraData, EPixelartBuffer bufferType)
        {
            var sourceResolution = cameraData.SourceResolution;
            switch (bufferType)
            {
                case EPixelartBuffer.Depth:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 24,
                        graphicsFormat = GraphicsFormat.None,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        dimension = TextureDimension.Tex2D
                    };

                case EPixelartBuffer.DepthNormal:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm, //16位精度够不够呢..
                        volumeDepth = 1,
                        msaaSamples = 1,
                        sRGB = true,
                        dimension = TextureDimension.Tex2D
                    };

                case EPixelartBuffer.Albedo:
                case EPixelartBuffer.Emission:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        sRGB = true,
                        dimension = TextureDimension.Tex2D
                    };

                // 单通道
                case EPixelartBuffer.SmoothnessMetallic:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R16G16_UNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        sRGB = true,
                        dimension = TextureDimension.Tex2D
                    };

                case EPixelartBuffer.OriginUV:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R16G16_UNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        sRGB = true,
                        dimension = TextureDimension.Tex2D
                    };
                    
                case EPixelartBuffer.Properties:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        sRGB = true,
                        dimension = TextureDimension.Tex2D
                    };

                case EPixelartBuffer.SpecularOutput:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        sRGB = true,
                        dimension = TextureDimension.Tex2D
                    };
            }

            return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
            {
                depthBufferBits = 0,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm,
                volumeDepth = 1,
                msaaSamples = 1,
                sRGB = true,
                dimension = TextureDimension.Tex2D
            };
        }
    }
}