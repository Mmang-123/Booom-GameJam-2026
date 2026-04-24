using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{

    public class RF_PixelartRender : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_PixelartShader;
        [SerializeField] private LayerMask m_OpaqueLayerMash = ~0;

        #region Debug
        [Header("Debug")]
        [SerializeField] private bool m_DebugOutput = false;
        [SerializeField] private EPixelartBuffer m_DebugOutputBuffer;
        [SerializeField] private Texture2D m_DebugLUT = null;
        #endregion

        #region Pass
        private RenderPass_PixelartRenderSetup m_PixelartSetupPass;
        private RenderPass_OpaqueRendering m_OpaqueRenderingPass;
        private RenderPass_PixelartShading m_PixelartShadingPass;
        private RenderPass_PixelartRenderCleanup m_PixelartCleanupPass;

        private RenderPass_BufferOutput m_BufferOutputPass;
        #endregion
        
        public override void Create()
        {
            m_PixelartSetupPass = new()
            {
                renderPassEvent = RenderPassEvent.BeforeRendering
            };

            m_OpaqueRenderingPass = new(m_OpaqueLayerMash, PShaderTag.Pixelart)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPrePasses
            };

            m_PixelartShadingPass = new(m_PixelartShader)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };

            m_PixelartCleanupPass = new()
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

            if (m_DebugOutput)
            {
                m_BufferOutputPass = new()
                {
                    renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
                };
                m_BufferOutputPass.SetOutputBufferType(m_DebugOutputBuffer);
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_PixelartShader == null)
                return;

            if (m_DebugLUT != null)
            {
                m_DebugLUT.filterMode = FilterMode.Point;
                Shader.SetGlobalTexture(PShaderPropertyID.PixelartLUT, m_DebugLUT);
                m_PixelartSetupPass.IsDebugLUTOn = true;
            }
            else
            {
                m_PixelartSetupPass.IsDebugLUTOn = false;
            }

            renderer.EnqueuePass(m_PixelartSetupPass);

            renderer.EnqueuePass(m_OpaqueRenderingPass);

            renderer.EnqueuePass(m_PixelartShadingPass);

            if (m_DebugOutput)
            {
                renderer.EnqueuePass(m_BufferOutputPass);
            }

            renderer.EnqueuePass(m_PixelartCleanupPass);
        }
    }

}