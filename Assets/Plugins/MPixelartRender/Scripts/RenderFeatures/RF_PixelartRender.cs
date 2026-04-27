using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{

    public enum EObstacleDebug
    {
        Off = -1, Mask = 0, SDF = 1, FracSDF = 2
    }

    public class RF_PixelartRender : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_PixelartShader;
        [SerializeField] private LayerMask m_OpaqueLayerMash = ~0;

        #region Debug
        [Header("Obstacle Debug")]
        [SerializeField] private EObstacleDebug m_ObstacleDebug = EObstacleDebug.Off;

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

#if UNITY_EDITOR
        private RenderPass_Blit m_DebugBlitPass;

#endif
        
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

            if (m_ObstacleDebug == EObstacleDebug.Off && m_DebugOutput)
            {
                m_BufferOutputPass = new()
                {
                    renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
                };
                m_BufferOutputPass.SetOutputBufferType(m_DebugOutputBuffer);
            }

            if (m_ObstacleDebug != EObstacleDebug.Off)
            {
                Shader obstacleDebugShader = Shader.Find("Hidden/Mmang/Pixelart/Blit/ObstacleDebug");
                m_DebugBlitPass = new(obstacleDebugShader)
                {
                    renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
                };
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

#if UNITY_EDITOR
            if (m_ObstacleDebug == EObstacleDebug.Off && m_DebugOutput)
            {
                renderer.EnqueuePass(m_BufferOutputPass);
            }

            if (m_ObstacleDebug != EObstacleDebug.Off)
            {
                if (m_DebugBlitPass.Material != null)
                    m_DebugBlitPass.Material.SetInt("_DebugType", (int)m_ObstacleDebug);
                renderer.EnqueuePass(m_DebugBlitPass);
            }

#endif

            renderer.EnqueuePass(m_PixelartCleanupPass);
        }
    }

}