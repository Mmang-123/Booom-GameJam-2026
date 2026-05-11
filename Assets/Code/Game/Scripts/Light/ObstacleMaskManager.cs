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
        [SerializeField] int m_NearestPointSearchRange = 16;

        [SerializeField] private Vector2Int m_ChunkRange = new(3, 3);


        private Vector2Int m_RealChunkRange;

        private RenderTexture m_Mask;
        private RTHandle m_MaskHandle;
        private RenderTexture m_Lighting;
        private RTHandle m_LightingHandle;

        public RTHandle MaskHandle => m_MaskHandle;
        public RTHandle LightingHandle => m_LightingHandle;

        private RenderTexture[] m_SDFs;
        private RTHandle[] m_SDFHandles;
        private int[] m_SDFThreadIDs;
        private RenderTexture m_SDFArray;
        private RenderTexture m_SDFIntermA;
        private RenderTexture m_SDFIntermB;

        public int Resolution => 256;
        public float TileSize => 16f;
        public float HalfTileSize => TileSize / 2f;
        public float UnitSize => TileSize / Resolution;
        public Vector2Int ChunkRange => m_RealChunkRange;

        private Camera m_Camera;

        //
        public Vector2Int CenterIndex { get; private set; }

        private void OnEnable()
        {
            InitChunkRange();
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

            if (m_SDFArray != null)  { m_SDFArray.Release();  m_SDFArray  = null; }
            if (m_SDFIntermA != null) { m_SDFIntermA.Release(); m_SDFIntermA = null; }
            if (m_SDFIntermB != null) { m_SDFIntermB.Release(); m_SDFIntermB = null; }
        }

        private void LateUpdate()
        {
            RenderMask();
            Shader.SetGlobalTexture(PShaderPropertyID.MLightingTexture, m_LightingHandle);
            Shader.SetGlobalTexture("_ObstacleSDF", m_SDFArray);
        }

        private void InitChunkRange()
        {
            int x = m_ChunkRange.x;
            int y = m_ChunkRange.y;
            if (x < 1)
                x = 1;
            else if (x % 2 == 0)
                x -= 1;
            
            if (y < 1)
                y = 1;
            else if (y % 2 == 0)
                y -= 1;
            
            m_RealChunkRange = new(x, y);
        }

        private void RegisterSDFThreads()
        {
            if (m_DFFeature == null) return;

            m_DFFeature.PendingBatch(new GenerateDFRendererFeature.DFBatchParams
            {
                sourceTexture          = null,   // use obstacle camera color buffer
                intermA                = m_SDFIntermA,
                intermB                = m_SDFIntermB,
                targetArray            = m_SDFArray,
                chunkRangeX            = m_RealChunkRange.x,
                chunkRangeY            = m_RealChunkRange.y,
                resolution             = Resolution,
                extendPixels           = 128,
                alphaThreshold         = 0.05f,
                nearestPointSearchRange = m_NearestPointSearchRange,
            });
        }

        private void ReleaseSDFThreads()
        {
            m_DFFeature?.ReleaseBatch();
        }

        private void CreateTextures()
        {
            var maskDescriptor = new RenderTextureDescriptor(Resolution * (m_RealChunkRange.x + 1), Resolution * (m_RealChunkRange.y + 1))
            {
                depthBufferBits = 32,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16_UNorm,
                volumeDepth = 1,
                msaaSamples = 1,
                sRGB = true,
                dimension = TextureDimension.Tex2D,
            };

            var lightingDescriptor = new RenderTextureDescriptor(Resolution * m_RealChunkRange.x, Resolution * m_RealChunkRange.y)
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


            int totalChunks = m_RealChunkRange.x * m_RealChunkRange.y;
            int iterSize    = Resolution + 2 * 128; // Resolution + 2 * extendPixels

            // Output SDF array (written directly by batch compute kernel)
            var arrayDescriptor = new RenderTextureDescriptor(Resolution, Resolution)
            {
                depthBufferBits = 0,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16_UNorm,
                volumeDepth = totalChunks,
                msaaSamples = 1,
                sRGB = false,
                dimension = TextureDimension.Tex2DArray,
            };
            m_SDFArray = new RenderTexture(arrayDescriptor)
            {
                name = "_ObstacleSDF",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            m_SDFArray.Create();

            // Intermediate ping/pong buffers for batch JFA (ARGBHalf = 8 bytes/pixel)
            var intermDescriptor = new RenderTextureDescriptor(iterSize, iterSize)
            {
                depthBufferBits = 0,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat,
                volumeDepth = totalChunks,
                msaaSamples = 1,
                sRGB = false,
                dimension = TextureDimension.Tex2DArray,
            };
            m_SDFIntermA = new RenderTexture(intermDescriptor) { name = "_SDFIntermA", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            m_SDFIntermA.Create();
            m_SDFIntermB = new RenderTexture(intermDescriptor) { name = "_SDFIntermB", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            m_SDFIntermB.Create();

            // Keep legacy individual RT arrays as empty stubs so existing code that reads
            // m_SDFs/m_SDFHandles doesn't null-ref (they're no longer used for SDF generation).
            m_SDFs      = System.Array.Empty<RenderTexture>();
            m_SDFHandles = System.Array.Empty<RTHandle>();
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
            // 3X3的判定
            /*
            chunkIndex -= CenterIndex;
            if (Mathf.Abs(chunkIndex.x) > 1 || Mathf.Abs(chunkIndex.y) > 1)
                return false;
            return true;
            */

            chunkIndex -= CenterIndex;
            if (Mathf.Abs(chunkIndex.x) > ((m_RealChunkRange.x - 1) / 2)
            || Mathf.Abs(chunkIndex.y) > ((m_RealChunkRange.y - 1) / 2))
                return false;
            return true;
        }

        public Vector2Int GetLocalChunkIndex(Vector2Int worldChunkIndex)
        {
            worldChunkIndex -= CenterIndex;
            //worldChunkIndex += Vector2Int.one;
            worldChunkIndex += new Vector2Int((m_RealChunkRange.x - 1) / 2, (m_RealChunkRange.y - 1) / 2);
            return worldChunkIndex;
        }

        public int GetChunkTextureIndex(Vector2Int chunkIndex)
        {
            chunkIndex -= CenterIndex;
            //chunkIndex += Vector2Int.one;
            chunkIndex += new Vector2Int((m_RealChunkRange.x - 1) / 2, (m_RealChunkRange.y - 1) / 2);
            return chunkIndex.x + chunkIndex.y * m_RealChunkRange.x;
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
            //camera.orthographicSize = TileSize * 4f / 2f;
            camera.orthographicSize = TileSize * (m_RealChunkRange.y + 1f) / 2f;

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