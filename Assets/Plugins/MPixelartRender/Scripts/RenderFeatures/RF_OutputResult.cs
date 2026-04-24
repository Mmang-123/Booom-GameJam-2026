using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RenderPass_OutputResult : ScriptableRenderPass
    {
        private static readonly string s_PassName = "Pixelart Output Result";

        private Material m_Material;
        private Shader m_Shader;

        public RenderPass_OutputResult(Shader shader)
        {
            m_Shader = shader;
            m_Material = new(shader);
        }

        private class PassData
        {
            public RTHandle Source;
            public Material BlitMaterial;
            public Vector4 Resolution;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
            var resourceData = frameContext.Get<UniversalResourceData>();
            var camera = cameraData.camera;
            var pixelartCamera = PixelartManager.Instance.GetPixelartCamera(camera, EPixelartCameraType.Result);
            if (pixelartCamera == null)
                return;
            if (m_Material == null)
                m_Material = new(m_Shader);

            var sourceResolution = pixelartCamera.CameraData.SourceResolution;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(s_PassName, out var passData))
            {
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                passData.Source = pixelartCamera.ResultBufferHandle;
                passData.BlitMaterial = m_Material;
                passData.Resolution = new Vector4(camera.pixelWidth, camera.pixelHeight, sourceResolution.x, sourceResolution.y);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            context.cmd.SetGlobalVector(PShaderPropertyID.Resolution, data.Resolution);
            if (data.BlitMaterial != null)
                Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), data.BlitMaterial, 0);
        }
    }

    public class RF_OutputResult : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_BlitShader;
        private RenderPass_OutputResult m_OutputResultPass;

        public override void Create()
        {
            if (m_BlitShader == null)
                return;
            m_OutputResultPass = new(m_BlitShader)
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_BlitShader == null)
                return;
            renderer.EnqueuePass(m_OutputResultPass);
        }
    }


}