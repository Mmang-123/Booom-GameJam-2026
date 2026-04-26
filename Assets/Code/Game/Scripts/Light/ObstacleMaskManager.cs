using Mmang.Util;
using Sloane;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    [ExecuteAlways]
    public class ObstacleMaskManager : SingletonMono<ObstacleMaskManager>
    {
        // 存储 9 张障碍物贴图
        private RenderTexture[] m_ObstacleRTs = new RenderTexture[9];
        private RTHandle[] m_MaskHandles = new RTHandle[9];

        private RenderTexture[] m_SDFs = new RenderTexture[9];
        private RTHandle[] m_SDFHandles = new RTHandle[9];

        public int Resolution => 256;
        public float TileSize => 16f;
        public float UnitSize => TileSize / Resolution;

        private Camera m_Camera;

        private void OnEnable()
        {
            InitCamera();
            CreateTextures();
        }

        private void OnDisable()
        {
            if (m_Camera != null)
            {
                DestroyImmediate(m_Camera.gameObject);
                m_Camera = null;
            }

            foreach (var rt in m_MaskHandles)
            {
                if (rt != null) rt.Release();
            }

            foreach (var rt in m_SDFHandles)
            {
                if (rt != null) rt.Release();
            }
        }

        private void Update()
        {
            RenderMask();
            GenerateSDF();
        }

        private void CreateTextures()
        {
            for (int i = 0; i < 9; i++)
            {
                if (m_MaskHandles[i] != null)
                    m_MaskHandles[i].Release();

                //
                var descriptor = new RenderTextureDescriptor(Resolution, Resolution)
                {
                    depthBufferBits = 32,
                    enableRandomWrite = true,
                    graphicsFormat = GraphicsFormat.R16_UNorm,
                    volumeDepth = 1,
                    msaaSamples = 1,
                    sRGB = true,
                    dimension = TextureDimension.Tex2D,
                };

                //
                m_ObstacleRTs[i] = new(descriptor)
                {
                    name = $"_ObstacleMask_{i}",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                m_ObstacleRTs[i].Create();
                m_MaskHandles[i] = RTHandles.Alloc(m_ObstacleRTs[i]);

                m_SDFs[i] = new(descriptor)
                {
                    name = $"_ObstacleSDF_{i}",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                m_SDFs[i].Create();
                m_SDFHandles[i] = RTHandles.Alloc(m_SDFs[i]);
            }
        }

        public Vector3 GetPositionByIndex(int index)
        {
            int x = index % 3;
            int y = index / 3;
            return new Vector3(x * TileSize, y * TileSize, -10);
        }

        public RTHandle GetMaskHandel(int index)
        {
            return m_MaskHandles[index];
        }

        public RTHandle GetSDFHandle(int index)
        {
            return m_SDFHandles[index];
        }


        #region 相机绘制

        private void InitCamera()
        {
            var cameraGO = new GameObject("Obstacle Camera");
            var camera = cameraGO.AddComponent<Camera>();
            var cameraData = cameraGO.AddComponent<UniversalAdditionalCameraData>();

            cameraData.requiresColorOption = CameraOverrideOption.Off;
            cameraData.requiresDepthOption = CameraOverrideOption.Off;
            cameraData.renderShadows = false;

            var t = transform;
            //camera.transform.SetPositionAndRotation(transform.position, t.rotation);
            camera.enabled = false;
            cameraGO.hideFlags = HideFlags.HideAndDontSave;

            camera.orthographic = true;
            camera.orthographicSize = TileSize / 2f;

            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.clear;

            cameraData.SetRenderer(PixelartRendererType.ObstacleMask);

            //
            m_Camera = camera;
        }

        private void RenderMask()
        {
            for (int i = 0; i < 9; i++)
            {
                RenderPipeline.StandardRequest request = new();

                m_Camera.transform.position = GetPositionByIndex(i);
                m_Camera.transform.rotation = quaternion.identity;

                if (RenderPipeline.SupportsRenderRequest(m_Camera, request))
                {
                    // 纹理绑定
                    request.destination = m_ObstacleRTs[i];

                    RenderPipeline.SubmitRenderRequest(m_Camera, request);
                }
            }
        }

        private void GenerateSDF()
        {
            for (int i = 0; i < 9; i++)
            {
                SDFToolsRuntime.GenerateSDF(m_ObstacleRTs[i], m_SDFHandles[i], boundaryDistance: true);
                Shader.SetGlobalTexture(Shader.PropertyToID($"_ObstacleSDF_{i}"), m_SDFHandles[i]);
            }
        }

        #endregion
    }
}