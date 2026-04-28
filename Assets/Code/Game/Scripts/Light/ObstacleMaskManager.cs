using Mmang.Util;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    [ExecuteAlways]
    public class ObstacleMaskManager : SingletonMono<ObstacleMaskManager>
    {
        public struct ChunkData
        {
            public Vector2Int PositionIndex;
        }

        [SerializeField] GenerateDFRendererFeature m_DFFeature;

        private RenderTexture m_Mask;
        private RTHandle m_MaskHandle;
        private RenderTexture m_Lighting;
        private RTHandle m_LightingHandle;
        public RTHandle LightingHandle => m_LightingHandle;

        private RenderTexture[] m_SDFs = new RenderTexture[9];
        private RTHandle[] m_SDFHandles = new RTHandle[9];
        private int[] m_SDFThreadIDs = new int[9];

        public int Resolution => 256;
        public float TileSize => 16f;
        public float HalfTileSize => TileSize / 2f;
        public float UnitSize => TileSize / Resolution;

        private Vector2Int m_CenterIndex;
        private ChunkData[] m_ChunkDataArray = new ChunkData[9];
        private Camera m_Camera;

        public Vector2Int CenterIndex => m_CenterIndex;

        private void OnEnable()
        {
            InitCamera();
            CreateTextures();
            RegisterSDFThreads();

            m_CenterIndex = Vector2Int.zero;
            UpdateChunk();
        }

        private void OnDisable()
        {
            ReleaseSDFThreads();

            if (m_Camera != null)
            {
                DestroyImmediate(m_Camera.gameObject);
                m_Camera = null;
            }

            if (m_MaskHandle != null) m_MaskHandle.Release();
            if (m_LightingHandle != null) m_LightingHandle.Release();

            foreach (var rt in m_SDFHandles)
                if (rt != null) rt.Release();
        }

        private void Update()
        {
            RenderMask();
            Shader.SetGlobalTexture(PShaderPropertyID.MLightingTexture, m_LightingHandle);
        }

        private void RegisterSDFThreads()
        {
            if (m_DFFeature == null) return;
            for (int i = 0; i < 9; i++)
            {
                Vector2Int offset = new(i % 3 * Resolution, i / 3 * Resolution);
                m_SDFThreadIDs[i] = m_DFFeature.Pending(
                    m_Mask, m_SDFs[i], offset,
                    extendPixels: 128, nearestPointSearchRange: 16, boundaryDistance: false);
            }
        }

        private void ReleaseSDFThreads()
        {
            if (m_DFFeature == null) return;
            for (int i = 0; i < 9; i++)
            {
                if (m_SDFThreadIDs[i] > 0)
                {
                    m_DFFeature.Release(m_SDFThreadIDs[i]);
                    m_SDFThreadIDs[i] = 0;
                }
            }
        }

        private void CreateTextures()
        {
            var maskDescriptor = new RenderTextureDescriptor(Resolution * 4, Resolution * 4)
            {
                depthBufferBits = 32,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16_UNorm,
                volumeDepth = 1,
                msaaSamples = 1,
                sRGB = true,
                dimension = TextureDimension.Tex2D,
            };

            var lightingDescriptor = new RenderTextureDescriptor(Resolution * 3, Resolution * 3)
            {
                depthBufferBits = 0,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm,
                volumeDepth = 1,
                msaaSamples = 1,
                sRGB = true,
                dimension = TextureDimension.Tex2D,
            };

            m_Mask = new(maskDescriptor)
            {
                name = $"_ObstacleMask",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            m_Mask.Create();
            m_MaskHandle = RTHandles.Alloc(m_Mask);

            m_Lighting = new(lightingDescriptor)
            {
                name = $"_LightingTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            m_Lighting.Create();
            m_LightingHandle = RTHandles.Alloc(m_Lighting);

            for (int i = 0; i < 9; i++)
            {
                var descriptor = new RenderTextureDescriptor(Resolution, Resolution)
                {
                    depthBufferBits = 0,
                    enableRandomWrite = true,
                    graphicsFormat = GraphicsFormat.R16_UNorm,
                    volumeDepth = 1,
                    msaaSamples = 1,
                    sRGB = true,
                    dimension = TextureDimension.Tex2D,
                };

                m_SDFs[i] = new(descriptor)
                {
                    name = $"_ObstacleSDF_{i}",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                m_SDFs[i].Create();
                m_SDFHandles[i] = RTHandles.Alloc(m_SDFs[i]);
                Shader.SetGlobalTexture(Shader.PropertyToID($"_ObstacleSDF_{i}"), m_SDFs[i]);
            }
        }


        public void UpdatePosition(Vector2 position)
        {
            Vector2 pos = position;
            Vector2Int index = new(Mathf.FloorToInt(pos.x / TileSize), Mathf.FloorToInt(pos.y / TileSize));
            if (index != m_CenterIndex)
            {
                m_CenterIndex = index;
                UpdateChunk();
            }
        }

        private void UpdateChunk()
        {
            Vector2Int leftBot = m_CenterIndex - Vector2Int.one;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    m_ChunkDataArray[i + j * 3] = new()
                    {
                        PositionIndex = leftBot + new Vector2Int(i, j)
                    };
                }
            }
        }

        public Vector3 GetPositionByIndex(int index)
        {
            var chunkData = m_ChunkDataArray[index];
            return new Vector3(chunkData.PositionIndex.x * TileSize + HalfTileSize, chunkData.PositionIndex.y * TileSize + HalfTileSize, -10);
        }

        public Vector3 GetCenterPosition()
        {
            return new Vector3(m_CenterIndex.x * TileSize + HalfTileSize, m_CenterIndex.y * TileSize + HalfTileSize, -10);
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

            camera.enabled = false;
            cameraGO.hideFlags = HideFlags.HideAndDontSave;

            camera.orthographic = true;
            camera.orthographicSize = TileSize * 4f / 2f;

            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.clear;

            cameraData.SetRenderer(PixelartRendererType.ObstacleMask);

            //
            m_Camera = camera;
        }

        private void RenderMask()
        {
            RenderPipeline.StandardRequest request = new();

            m_Camera.transform.position = GetCenterPosition();
            m_Camera.transform.rotation = quaternion.identity;

            if (RenderPipeline.SupportsRenderRequest(m_Camera, request))
            {
                // 纹理绑定
                request.destination = m_MaskHandle;

                RenderPipeline.SubmitRenderRequest(m_Camera, request);
            }

            Shader.SetGlobalTexture(Shader.PropertyToID("_ObstacleMask"), m_MaskHandle);
        }

        #endregion
    }
}