using Mmang.PixelartRender;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public class ScreenTransitionRendererFeaturePass : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Screen Transition";
        public static readonly string TempTextureName = "ScreenTransitionTempTexture";    

        private class PassData
        {
            public TextureHandle Source;
            public Material Material;
        }

        //
        private Shader m_Shader;
        private Material m_Material;

        public ScreenTransitionRendererFeaturePass(Shader inShader)
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
                passData.Material = m_Material;
                passData.Source = resourceData.activeColorTexture;

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
            Blitter.BlitTexture(cmd, passData.Source, new Vector4(1, 1, 0, 0), passData.Material, 0);
        }
    }
}