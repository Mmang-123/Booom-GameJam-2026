using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RF_Lighting : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_LightingShader;

        private RenderPass_Lighting m_LightingPass;

        public override void Create()
        {
            m_LightingPass = new(m_LightingShader)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques - 1
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_LightingPass);
        }

    }
}