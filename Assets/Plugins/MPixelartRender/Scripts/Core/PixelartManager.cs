using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Mmang.PixelartRender
{
    public class PixelartManager : SingletonMono<PixelartManager>
    {
        private Dictionary<Camera, PixelartCamera> m_ResultCameraMap = new();
        private Dictionary<Camera, PixelartCamera> m_CastCameraMap = new();

        #region 相机注册
        public void Register(PixelartCamera pixelartCamera)
        {
            if (pixelartCamera == null || pixelartCamera.CastCamera == null)
                return;

            m_ResultCameraMap.Add(pixelartCamera.Camera, pixelartCamera);
            m_CastCameraMap.Add(pixelartCamera.CastCamera.Camera, pixelartCamera);
        }

        public void Unregister(PixelartCamera pixelartCamera)
        {
            if (pixelartCamera == null || pixelartCamera.CastCamera == null)
                return;

            m_ResultCameraMap.Remove(pixelartCamera.Camera);
            m_CastCameraMap.Remove(pixelartCamera.CastCamera.Camera);
        }
        #endregion


        #region 原生相机相关

        public bool IsResultCamera(Camera camera)
        {
            return m_ResultCameraMap.ContainsKey(camera);
        }

        public bool IsCastCamera(Camera camera)
        {
            return m_CastCameraMap.ContainsKey(camera);
        }

        public EPixelartCameraType GetPixelartCameraType(Camera camera)
        {
            if (m_ResultCameraMap.ContainsKey(camera))
                return EPixelartCameraType.Result;
            if (m_CastCameraMap.ContainsKey(camera))
                return EPixelartCameraType.Cast;
            return EPixelartCameraType.None;
        }

        public PixelartCamera GetPixelartCamera(Camera camera)
        {
            if (m_ResultCameraMap.TryGetValue(camera, out var result))
                return result;
            if (m_CastCameraMap.TryGetValue(camera, out result))
                return result;
            return null;
        }
        public PixelartCamera GetPixelartCamera(Camera camera, EPixelartCameraType inCameraType)
        {
            if (inCameraType == EPixelartCameraType.Result)
            {
                if (m_ResultCameraMap.TryGetValue(camera, out var result))
                    return result;
            }
            if (inCameraType == EPixelartCameraType.Cast)
            {
                if (m_CastCameraMap.TryGetValue(camera, out var result))
                    return result;
            }
            return null;
        }

        public PixelartCastCamera GetPixelartCastCamera(Camera camera)
        {
            var pixelartCamera = GetPixelartCamera(camera);
            if (pixelartCamera != null)
                return pixelartCamera.CastCamera;
            return null;
        }
        public PixelartCastCamera GetPixelartCastCamera(Camera camera, EPixelartCameraType inCameraType)
        {
            var pixelartCamera = GetPixelartCamera(camera, inCameraType);
            if (pixelartCamera != null)
                return pixelartCamera.CastCamera;
            return null;
        }

        #endregion


    }
}