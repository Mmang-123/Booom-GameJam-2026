using System;
using Mmang.Util;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mmang.PixelartRender
{
    [ExecuteAlways, DisallowMultipleComponent]
    public class PixelartCastCamera : MonoBehaviour
    {
        private PixelartCamera m_ParentCamera;
        private Camera m_Camera;

        public PixelartCamera ParentCamera => m_ParentCamera;
        public Camera Camera => m_Camera;
        public UniversalAdditionalCameraData AdditionalCameraData { get; private set; }

        public void Init(PixelartCamera pixelartCamera)
        {
            m_ParentCamera = pixelartCamera;
            if (!TryGetComponent(out m_Camera))
                m_Camera = gameObject.AddComponent<Camera>();

            AdditionalCameraData = m_Camera.GetUniversalAdditionalCameraData();
            AdditionalCameraData.SetRenderer(PixelartRendererType.CastCamera);

            RefreshCamera();
        }

        public void RefreshCamera()
        {
            CameraUtil.CopyCameraProperties(m_ParentCamera.Camera, m_Camera);
        }
    }



}