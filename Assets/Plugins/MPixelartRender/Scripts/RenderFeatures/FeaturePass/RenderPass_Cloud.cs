using Mmang.PixelartRender.VolumeComponents;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RenderPass_Cloud : ScriptableRenderPass
    {
        private static readonly string s_PassTag = "Cloud";
        private class PassData
        {
            public Material Material;
            public float CloudSize;
        }

        private Material m_CloudMaterial;
        public float CloudSize = 10f;
        public Vector2Int Resolution = new(628, 628);


        #region Cloud Texture

        RenderTextureDescriptor CloudTextureDescriptor => new(Resolution.x, Resolution.y)
        {
            depthBufferBits = 0,
            enableRandomWrite = true,
            graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm,
            volumeDepth = 1,
            msaaSamples = 1,
            sRGB = true,
            dimension = TextureDimension.Tex2D
        };

        string CloudTextureName => "_CloudTexture";

        #endregion

        public void SetMaterial(Material material)
        {
            m_CloudMaterial = material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_CloudMaterial == null)
                return;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassTag, out var passData))
            {
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                var cloudComponent = VolumeManager.instance.stack.GetComponent<Cloud>();
                if (cloudComponent == null || !cloudComponent.IsActive())
                {
                    var whiteTexture = renderGraph.defaultResources.whiteTexture;
                    builder.SetGlobalTextureAfterPass(whiteTexture, PShaderPropertyID.CloudTexture);

                    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => { });
                }
                else
                {
                    // PassData
                    passData.Material = m_CloudMaterial;
                    passData.CloudSize = CloudSize;

                    // 绘制
                    TextureHandle cloudTextureHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, CloudTextureDescriptor, CloudTextureName, false);
                    builder.SetRenderAttachment(cloudTextureHandle, 0, AccessFlags.Write);
                    builder.SetGlobalTextureAfterPass(cloudTextureHandle, PShaderPropertyID.CloudTexture);

                    builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                    {
                        ExecutePass(rgContext.cmd, data);
                    });
                }
            }
        }

        private static void ExecutePass(RasterCommandBuffer cmd, PassData passData)
        {
            cmd.EnableKeyword(PShaderKeyword.Cloud);
            cmd.SetGlobalFloat(PShaderPropertyID.CloudSize, passData.CloudSize);
            Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), passData.Material, 0);
        }
    }
}