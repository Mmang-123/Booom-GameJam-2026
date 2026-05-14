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
            m_Camera.orthographic = true;
            m_Camera.orthographicSize = m_ParentCamera.MaxOrthographSize;
            m_Camera.nearClipPlane = m_ParentCamera.Camera.nearClipPlane;
            m_Camera.farClipPlane = m_ParentCamera.Camera.farClipPlane;
            m_Camera.backgroundColor = m_ParentCamera.Camera.backgroundColor;
        }
    }



}