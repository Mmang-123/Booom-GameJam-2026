using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RF_GodRay : ScriptableRendererFeature
    {
        [SerializeField] private Mesh m_Mesh;
        [SerializeField] private Shader m_Shader;
        [SerializeField] private RenderPassEvent m_RenderPassEvent;

        //
        private RenderPass_GodRay m_GodRayPass;

        public override void Create()
        {
            if (m_Shader == null || m_Mesh == null)
                return;

            m_GodRayPass = new(m_Mesh, m_Shader)
            {
                renderPassEvent = m_RenderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_GodRayPass != null)
                renderer.EnqueuePass(m_GodRayPass);
        }
    }

}