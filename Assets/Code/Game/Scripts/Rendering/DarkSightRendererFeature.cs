using Mmang.PixelartRender;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public class DarkSightRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_Shader;
        [SerializeField] private RenderPassEvent m_RenderPassEvent;

        private DarkSightRendererFeaturePass m_Pass;
        private RenderPass_TempTextureBlitBack m_BlitBackPass;

        public override void Create()
        {
            m_Pass = new(m_Shader)
            {
                renderPassEvent = m_RenderPassEvent
            };
            m_BlitBackPass = new(DarkSightRendererFeaturePass.TempTextureName)
            {
                renderPassEvent = m_RenderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_Shader != null)
            {
                renderer.EnqueuePass(m_Pass);
                renderer.EnqueuePass(m_BlitBackPass);   
            }
        }
    }
}