using Mmang.PixelartRender;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public class DarkSightRendererFeaturePass : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Dark Sight";
        public static readonly string TempTextureName = "DarkSightTempTexture";    

        static readonly int Property_DarkSightParams = Shader.PropertyToID("_DarkSightParams");

        private class PassData
        {
            public TextureHandle Source;
            public Material Material;
            public Vector4 Params;
        }

        //
        private Shader m_Shader;
        private Material m_Material;

        public DarkSightRendererFeaturePass(Shader inShader)
        {
            m_Shader = inShader;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Shader == null)
                return;
            if (m_Material == null)
                m_Material = new(m_Shader);
            
            var tempTextureData = frameData.GetOrCreate<TempTextureData>();
            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassTag, out var passData))
            {
                var param = DarkSightManager.Instance.GetParams();
                passData.Material = m_Material;
                passData.Source = resourceData.activeColorTexture;
                passData.Params = new(param.uv.x, param.uv.y, param.radiusRatio.x, param.radiusRatio.y);

                RenderTextureDescriptor resultDesc = cameraData.cameraTargetDescriptor;
                resultDesc.depthBufferBits = 0;
                TextureHandle resultTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, resultDesc, TempTextureName, true, FilterMode.Point);
                builder.SetRenderAttachment(resultTexture, 0, AccessFlags.Write);
                tempTextureData.StoreTexture(TempTextureName, resultTexture);

                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    ExecutePass(rgContext.cmd, data);
                });
            }
        }

        private static void ExecutePass(RasterCommandBuffer cmd, PassData passData)
        {
            passData.Material.SetVector(Property_DarkSightParams, passData.Params);
            Blitter.BlitTexture(cmd, passData.Source, new Vector4(1, 1, 0, 0), passData.Material, 0);
        }
    }
}