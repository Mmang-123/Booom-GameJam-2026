using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RF_ObstacleMask : ScriptableRendererFeature
    {
        [SerializeField] private RenderPassEvent m_RenderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [SerializeField] private LayerMask m_LayerMash = ~0;

        private RenderPass_ObstacleMask m_ForwardPass;
        

        public override void Create()
        {
            m_ForwardPass = new(m_LayerMash, PShaderTag.ObstacleMask)
            {
                renderPassEvent = m_RenderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ForwardPass);
        }
    }

}