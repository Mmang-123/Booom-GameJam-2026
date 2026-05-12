using Mmang.PixelartRender;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public class ScreenTransitionRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_Shader;
        [SerializeField] private Shader m_BlitBackShader;
        [SerializeField] private RenderPassEvent m_RenderPassEvent;

        private ScreenTransitionRendererFeaturePass m_Pass;
        private RenderPass_TempTextureBlitBack m_BlitBackPass;

        public override void Create()
        {
            m_Pass = new(m_Shader)
            {
                renderPassEvent = m_RenderPassEvent
            };
            m_BlitBackPass = new(ScreenTransitionRendererFeaturePass.TempTextureName)
            {
                renderPassEvent = m_RenderPassEvent
            };
            m_BlitBackPass.SetShader(m_BlitBackShader);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_Shader != null && GameManager.Instance.ScreenFadeT > 0f)
            {
                Shader.SetGlobalFloat("_SceneTransition", GameManager.Instance.ScreenFadeT);
                renderer.EnqueuePass(m_Pass);
                renderer.EnqueuePass(m_BlitBackPass);   
            }
        }
    }
}