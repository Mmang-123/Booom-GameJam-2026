using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    public class RF_Cloud : ScriptableRendererFeature
    {
        [SerializeField] private Material m_CloudMaterial;
        [SerializeField] private float m_CloudSize = 10f;
        [SerializeField] private Vector2Int m_Resolution = new(628, 628);

        private RenderPass_Cloud m_CloudPass;

        public override void Create()
        {
            m_CloudPass = new()
            {
                renderPassEvent = RenderPassEvent.BeforeRendering,
                CloudSize = m_CloudSize,
                Resolution = m_Resolution
            };
            m_CloudPass.SetMaterial(m_CloudMaterial);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_CloudMaterial == null)
                return;

            renderer.EnqueuePass(m_CloudPass);
        }
    }

}