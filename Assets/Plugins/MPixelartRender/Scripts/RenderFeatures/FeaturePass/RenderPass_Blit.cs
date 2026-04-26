using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RenderPass_Blit : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Obstacle Debug";
        public static readonly string TempTextureName = "HaloTempTexture";    

        private class PassData
        {
            public Material Material;
        }

        private Shader m_Shader;
        private Material m_Material;
        public Material Material => m_Material;

        public RenderPass_Blit(Shader shader)
        {
            m_Shader = shader;
        }

        ~RenderPass_Blit()
        {
            if (m_Material != null)
            {
                Object.Destroy(m_Material);
            }
        }

        private bool CheckMaterial()
        {
            if (m_Shader == null)
            {
                return false;
            }

            if (m_Material == null && m_Shader != null)
            {
                m_Material = new(m_Shader);
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
                passData.Material = m_Material;

                builder.AllowPassCulling(false);

                //
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    ExecutePass(rgContext.cmd, data);
                });
            }
        }

        private static void ExecutePass(RasterCommandBuffer cmd, PassData passData)
        {
            Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), passData.Material, 0);
        }
    }
}