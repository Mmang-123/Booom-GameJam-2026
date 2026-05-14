using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{

    public enum EPixelartCameraType
    {
        None, Result, Cast
    }

    [System.Serializable]
    public struct PixelartCameraData
    {
        public Vector2Int SourceResolution;

        public static PixelartCameraData Default => new()
        {
            SourceResolution = new(320, 180)
        };
    }

    [ExecuteAlways, DisallowMultipleComponent]
    [RequireComponent(typeof(Camera), typeof(UniversalAdditionalCameraData))]
    public class PixelartCamera : MonoBehaviour
    {
        #region 设置
        //[SerializeField] private Vector2Int m_TargetResolution = new(320, 180);
        [SerializeField] private PixelartCameraData m_CameraData = PixelartCameraData.Default;
        public PixelartCameraData CameraData => m_CameraData;

        public float UnitSize => CastCamera.Camera.orthographicSize * 2f / m_CameraData.SourceResolution.y;

        #endregion


        #region Buffer

        public RenderTexture ResultBuffer { get; private set; }
        public RTHandle ResultBufferHandle { get; private set; }

        #endregion

        #region 运行时属性

        [SerializeField] private float m_OrthographicSize = 10f;
        [SerializeField, Range(0f, 1f)] private float m_CameraScale = 1;
        public float MaxOrthographSize => m_OrthographicSize;
        public float CameraScale => m_CameraScale;

        #endregion

        public Camera Camera { get; private set; }
        public PixelartCastCamera CastCamera { get; private set; }
        public UniversalAdditionalCameraData AdditionalCameraData { get; private set; }

        #region Mono

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            Dispose();
        }

        #endregion

        //
        private void Init()
        {
            InitResultBuffer();
            InitResultCamera();
            InitCastCamera();

            PixelartManager.Instance.Register(this);
        }

        //
        private void Dispose()
        {
            if (PixelartManager.InstanceValid)
            {
                PixelartManager.Instance.Unregister(this);
            }

            DisposeResultBuffer();
        }

        private void InitResultBuffer()
        {
            var resolution = m_CameraData.SourceResolution;
            RenderTextureDescriptor resultDesc = new(resolution.x, resolution.y)
            {
                depthBufferBits = 24,
                stencilFormat = GraphicsFormat.R8_UInt,
                enableRandomWrite = true,
                graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR),
                sRGB = true,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D
            };

            if (ResultBuffer == null)
            {
                ResultBuffer = new(resultDesc)
                {
                    filterMode = FilterMode.Point
                };
                ResultBuffer.Create();
            }
            ResultBufferHandle = RTHandles.Alloc(ResultBuffer);
        }
        
        private void DisposeResultBuffer()
        {
            if (ResultBuffer != null)
            {
                ResultBuffer.Release();
                ResultBufferHandle.Release();
                ResultBuffer = null;
                ResultBufferHandle = null;
            }
        }

        private void InitResultCamera()
        {
            Camera = GetComponent<Camera>();
            //Camera.cullingMask = 0;
            AdditionalCameraData = GetComponent<UniversalAdditionalCameraData>();
            AdditionalCameraData.SetRenderer(PixelartRendererType.ResultCamera);
        }
        
        private void InitCastCamera()
        {
            CastCamera = GetComponentInChildren<PixelartCastCamera>();
            if (CastCamera == null)
            {
                var newGo = new GameObject("Cast Camera");
                newGo.transform.SetParent(transform, false);
                CastCamera = newGo.AddComponent<PixelartCastCamera>();
            }

            CastCamera.Init(this);
            CastCamera.Camera.targetTexture = ResultBuffer;
        }

        private void Update()
        {
            if (!Application.isPlaying && Camera != null)
            {
                Camera.orthographicSize = m_OrthographicSize * m_CameraScale;
            }
        }

        public void SetCameraScale(float scale)
        {
            m_CameraScale = Mathf.Clamp01(scale);
            Camera.orthographicSize = m_OrthographicSize * m_CameraScale;
        }
    }

}