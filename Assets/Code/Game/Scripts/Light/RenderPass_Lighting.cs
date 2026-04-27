using System.Collections.Generic;
using Mmang.PixelartRender.VolumeComponents;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RenderPass_Lighting : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Lighting";
        private class PassData
        {
            public Material Material;
            public ComputeBuffer DataBuffer;
            public int LightCount;
            public int PointLightCount;
        }

        private Shader m_Shader;
        private Material m_LightingMaterial;

        #region Texture

        string LightingTextureName => "_LightingTexture";

        #endregion

        public RenderPass_Lighting(Shader shader)
        {
            m_Shader = shader;   
        }

        public void SetMaterial(Material material)
        {
            m_LightingMaterial = material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Shader == null)
                return;
            if (m_LightingMaterial == null)
                m_LightingMaterial = new(m_Shader);

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassTag, out var passData))
            {
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                if (false) // 需要的话，可以后面加个开关之类的
                {
                    /*
                    var whiteTexture = renderGraph.defaultResources.whiteTexture;
                    builder.SetGlobalTextureAfterPass(whiteTexture, PShaderPropertyID.CloudTexture);

                    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => { });
                    */
                }
                else
                {
                    var manager = LightingManager.Instance;
                    // PassData
                    passData.Material = m_LightingMaterial;
                    passData.DataBuffer = manager.DataBuffer;
                    passData.LightCount = manager.LightCount;
                    passData.PointLightCount = manager.PointLightCount;

                    // 绘制
                    var descriptor = cameraData.cameraTargetDescriptor;
                    descriptor.depthBufferBits = 0;
                    TextureHandle lightingTextureHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, LightingTextureName, false);
                    builder.SetRenderAttachment(lightingTextureHandle, 0, AccessFlags.Write);
                    builder.SetGlobalTextureAfterPass(lightingTextureHandle, PShaderPropertyID.MLightingTexture);

                    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                    {
                        ExecutePass(rgContext.cmd, data);
                    });
                }
            }
        }

        private static void ExecutePass(RasterCommandBuffer cmd, PassData passData)
        {
            cmd.SetGlobalBuffer(PShaderPropertyID.MLightDataBuffer, passData.DataBuffer);
            //cmd.SetGlobalInt(PShaderPropertyID.MLightCount, passData.LightCount);
            cmd.SetGlobalVector(PShaderPropertyID.MLightParams, new(passData.LightCount, passData.PointLightCount, 0, 0));
            Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), passData.Material, 0);
        }
    }
}