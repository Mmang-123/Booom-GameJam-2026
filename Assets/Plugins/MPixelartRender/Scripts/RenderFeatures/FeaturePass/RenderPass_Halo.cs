using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RenderPass_Halo : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Halo";
        public static readonly string TempTextureName = "HaloTempTexture";    

        private class PassData
        {
            public Material Material;
            public TextureHandle Source;
            public int Step;
        }

        private Shader m_HaloShader;
        private Material m_HaloMaterial;

        public RenderPass_Halo(Shader shader)
        {
            m_HaloShader = shader;
        }

        ~RenderPass_Halo()
        {
            if (m_HaloMaterial != null)
            {
                Object.Destroy(m_HaloMaterial);
            }
        }

        private bool CheckMaterial()
        {
            if (m_HaloShader == null)
            {
                return false;
            }

            if (m_HaloMaterial == null && m_HaloShader != null)
            {
                m_HaloMaterial = new(m_HaloShader);
            }

            return true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (!CheckMaterial())
                return;

            var tempTextureData = frameData.GetOrCreate<TempTextureData>();
            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassTag, out var passData))
            {
                passData.Material = m_HaloMaterial;
                passData.Source = resourceData.activeColorTexture;

                builder.AllowPassCulling(false);

                RenderTextureDescriptor resultDesc = cameraData.cameraTargetDescriptor;
                resultDesc.depthBufferBits = 0;
                TextureHandle resultTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, resultDesc, TempTextureName, true, FilterMode.Point);
                tempTextureData.StoreTexture(TempTextureName, resultTexture);

                //
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
                builder.SetRenderAttachment(resultTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    ExecutePass(rgContext.cmd, data);
                });
            }
        }

        private static void ExecutePass(RasterCommandBuffer cmd, PassData passData)
        {
            Blitter.BlitTexture(cmd, passData.Source, new Vector4(1, 1, 0, 0), passData.Material, 0);
        }
    }
}