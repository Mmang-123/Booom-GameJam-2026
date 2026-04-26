using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RF_Lighting : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_LightingShader;

        private RenderPass_ObstacleMask[] m_ObstacleMaskPasses = new RenderPass_ObstacleMask[9];
        private RenderPass_Lighting m_LightingPass;

        public override void Create()
        {
            /*
            for (int i = 0; i < 2; i++)
            {
                m_ObstacleMaskPasses[i] = new(i)
                {
                    renderPassEvent = RenderPassEvent.AfterRenderingOpaques - 1
                };   
            }
            */

            m_LightingPass = new(m_LightingShader)
            {
                // 在shading前
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques - 1
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            /*
            for (int i = 0; i < 1; i++)
            {
                renderer.EnqueuePass(m_ObstacleMaskPasses[i]);   
            }
            */
            renderer.EnqueuePass(m_LightingPass);
        }

    }
}