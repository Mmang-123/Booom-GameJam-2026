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
        [SerializeField] GenerateDFRendererFeature m_DFFeature;

        private RenderTexture m_Mask;
        private RTHandle m_MaskHandle;
        private RenderTexture m_Lighting;
        private RTHandle m_LightingHandle;

        public RTHandle MaskHandle => m_MaskHandle;
        public RTHandle LightingHandle => m_LightingHandle;

        private RenderTexture[] m_SDFs = new RenderTexture[9];
        private RTHandle[] m_SDFHandles = new RTHandle[9];
        private int[] m_SDFThreadIDs = new int[9];

        public int Resolution => 256;
        public float TileSize => 16f;
        public float HalfTileSize => TileSize / 2f;
        public float UnitSize => TileSize / Resolution;

        private Camera m_Camera;

        //
        public Vector2Int CenterIndex { get; private set; }

        private void OnEnable()
        {
            InitCamera();
            CreateTextures();
            RegisterSDFThreads();
            Application.targetFrameRate = 60;

            CenterIndex = Vector2Int.zero;
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
                    null, m_SDFs[i], offset,
                    extendPixels: 128, nearestPointSearchRange: 16, boundaryDistance: false, shaderPropertyID: Shader.PropertyToID($"_ObstacleSDF_{i}"));
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
            }
        }


        #region Chunk

        public Vector2Int GetChunkIndex(Vector2 worldPosition)
        {
            Vector2Int index = new(Mathf.FloorToInt(worldPosition.x / TileSize), Mathf.FloorToInt(worldPosition.y / TileSize));
            return index;
        }

        public Vector2Int GetChunkIndex(Vector2 worldPosition, out Vector2 offsetInChunk)
        {
            Vector2Int index = new(Mathf.FloorToInt(worldPosition.x / TileSize), Mathf.FloorToInt(worldPosition.y / TileSize));
            
            Vector2 origin = new(TileSize * index.x, TileSize * index.y);
            Vector2 offset = worldPosition - origin;
            offsetInChunk = new(offset.x / TileSize, offset.y / TileSize);

            return index;
        }

        public void UpdatePosition(Vector2 position)
        {
            Vector2Int index = GetChunkIndex(position);
            if (index != CenterIndex)
            {
                CenterIndex = index;
            }
        }

        public Vector3 GetCenterPosition()
        {
            return new Vector3(CenterIndex.x * TileSize + HalfTileSize, CenterIndex.y * TileSize + HalfTileSize, -10);
        }

        public bool IsVaildChunk(Vector2Int chunkIndex)
        {
            chunkIndex -= CenterIndex;
            if (Mathf.Abs(chunkIndex.x) > 1 || Mathf.Abs(chunkIndex.y) > 1)
                return false;
            return true;
        }

        public bool IsLastValidChunk(Vector2Int chunkIndex)
        {
            chunkIndex -= CenterIndex;
            if (Mathf.Abs(chunkIndex.x) > 1 || Mathf.Abs(chunkIndex.y) > 1)
                return false;
            return true;
        }

        public int GetChunkTextureIndex(Vector2Int chunkIndex)
        {
            chunkIndex -= CenterIndex;
            chunkIndex += Vector2Int.one;
            return chunkIndex.x + chunkIndex.y * 3;
        }

        #endregion


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