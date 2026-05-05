using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RenderPass_PixelartShading : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Pixelart Shading";

        private class PassData
        {
            public Material Material;
        }

        private Shader m_Shader;
        private Material m_Material;

        public RenderPass_PixelartShading(Shader inShader)
        {
            m_Shader = inShader;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Shader == null)
                return;
            if (m_Material == null)
                m_Material = new(m_Shader);

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            var camera = cameraData.camera;
            var pixelartCamera = PixelartManager.Instance.GetPixelartCamera(camera, EPixelartCameraType.Cast);
            if (pixelartCamera == null) return;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassTag, out var passData))
            {
                passData.Material = m_Material;

                builder.AllowPassCulling(false);

                // Color0: activeColor
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                // Color1: 高光
                TextureHandle specularBufferHandle = UniversalRenderer.CreateRenderGraphTexture
                (
                    renderGraph,
                    PBuffer.GetBufferDescriptor(pixelartCamera.CameraData, EPixelartBuffer.SpecularOutput),
                    PBuffer.GetBufferName(EPixelartBuffer.SpecularOutput),
                    false
                );
                builder.SetRenderAttachment(specularBufferHandle, 1, AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(specularBufferHandle, PBuffer.GetBufferShaderProperty(EPixelartBuffer.SpecularOutput));

                for (EPixelartBuffer bufferType = PRenderStage.RawDataStart; bufferType <= PRenderStage.RawDataEnd; bufferType++)
                {
                    builder.UseGlobalTexture(PBuffer.GetBufferShaderProperty(bufferType));
                }
                builder.UseGlobalTexture(PBuffer.GetBufferShaderProperty(EPixelartBuffer.Depth));

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }

        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            Blitter.BlitTexture(context.cmd, new Vector4(1, 1, 0, 0), data.Material, 0);
        }
    }
}