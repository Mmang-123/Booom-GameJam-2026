using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public class VelocityBufferFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_ReprojectShader;
        [SerializeField] private RenderPassEvent m_RenderPassEvent = RenderPassEvent.AfterRenderingOpaques;

        private Material m_ReprojectMaterial;
        private RTHandle m_ReadRT;
        private RTHandle m_WriteRT;
        private VelocityBufferFeaturePass m_Pass;

        // 相机追踪
        private Vector3 m_LastCameraPosition;
        private bool m_HasLastCameraPosition;

        private const int k_BufferWidth = 240;
        private const int k_BufferHeight = 135;
        private static readonly int s_ShaderID_CameraDelta = Shader.PropertyToID("_CameraDelta");
        private static readonly int s_ShaderID_CameraWorldSize = Shader.PropertyToID("_CameraWorldSize");

        public override void Create()
        {
            if (m_ReprojectShader == null)
            {
                Debug.LogError("[VelocityBufferFeature] Reproject Shader 未指定！");
                return;
            }

            m_ReprojectMaterial = new Material(m_ReprojectShader)
            {
                name = "VelocityReproject_Material"
            };

            var desc = new RenderTextureDescriptor(k_BufferWidth, k_BufferHeight,
                RenderTextureFormat.ARGBHalf, 0)
            {
                depthBufferBits = 0,
                msaaSamples = 1,
                useMipMap = false,
                autoGenerateMips = false
            };

            m_ReadRT = RTHandles.Alloc(desc, name: "_VelocityBuffer_A");
            m_WriteRT = RTHandles.Alloc(desc, name: "_VelocityBuffer_B");

            // 初始清空两个 RT
            ClearRT(m_ReadRT);
            ClearRT(m_WriteRT);

            m_Pass = new VelocityBufferFeaturePass(m_ReadRT, m_WriteRT, m_ReprojectMaterial)
            {
                renderPassEvent = m_RenderPassEvent
            };

            m_HasLastCameraPosition = false;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_Pass == null || m_ReprojectMaterial == null)
                return;

            var camera = renderingData.cameraData.camera;

            // -- 计算相机世界范围 --
            float worldHeight = camera.orthographicSize * 2f;
            float worldWidth = worldHeight * camera.aspect;
            var cameraWorldSize = new Vector2(worldWidth, worldHeight);

            // -- 计算相机位移 --
            Vector3 currentPos = camera.transform.position;
            currentPos.x = Mathf.Floor(currentPos.x * k_BufferWidth) / k_BufferWidth;
            currentPos.y = Mathf.Floor(currentPos.y * k_BufferHeight) / k_BufferHeight;
            Vector2 cameraDelta = Vector2.zero;
            if (m_HasLastCameraPosition)
            {
                Vector3 delta = currentPos - m_LastCameraPosition;
                cameraDelta = new Vector2(delta.x, delta.y);
            }
            m_LastCameraPosition = currentPos;
            m_HasLastCameraPosition = true;

            // -- 传给 Pass --
            m_Pass.SetCameraParams(cameraDelta, cameraWorldSize);

            // -- 设置 reproject material 参数 (blit 在 pass 内执行) --
            m_ReprojectMaterial.SetVector(s_ShaderID_CameraDelta, cameraDelta);
            m_ReprojectMaterial.SetVector(s_ShaderID_CameraWorldSize, cameraWorldSize);

            renderer.EnqueuePass(m_Pass);

            // -- 下一帧交换读写 --
            SwapRTs();
        }

        /// <summary>Pass 执行后交换读写 RT，写 RT 变为下帧的读 RT</summary>
        private void SwapRTs()
        {
            (m_ReadRT, m_WriteRT) = (m_WriteRT, m_ReadRT);
            m_Pass?.SwapRTs(m_ReadRT, m_WriteRT);
        }

        private static void ClearRT(RTHandle rt)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt.rt;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = prev;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Pass = null;
                m_ReadRT?.Release();
                m_WriteRT?.Release();
                m_ReadRT = null;
                m_WriteRT = null;
                CoreUtils.Destroy(m_ReprojectMaterial);
                m_ReprojectMaterial = null;
            }
            base.Dispose(disposing);
        }
    }
}
