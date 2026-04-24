using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RF_Halo : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_Shader;
        //[SerializeField] private int m_Step = 5;

        private RenderPass_Halo m_HaloPass;
        private RenderPass_TempTextureBlitBack m_HaloBlitBackPass;

        public override void Create()
        {
            m_HaloPass = new(m_Shader)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
            m_HaloBlitBackPass = new(RenderPass_Halo.TempTextureName)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_Shader != null)
            {
                renderer.EnqueuePass(m_HaloPass);
                renderer.EnqueuePass(m_HaloBlitBackPass);
            }
        }
    }
}