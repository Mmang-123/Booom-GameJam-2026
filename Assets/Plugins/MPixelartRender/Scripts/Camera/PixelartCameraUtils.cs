using Mmang.Topdown;
using UnityEngine;

namespace Mmang.PixelartRender
{
    public static class PixelartCameraUtils
    {
        public static Camera GetResultCamera(Camera camera)
        {
            var pixelartCamera = PixelartManager.Instance.GetPixelartCamera(camera);
            return pixelartCamera == null ? null : pixelartCamera.Camera;
        }

        public static Camera GetCastCamera(Camera camera)
        {
            var pixelartCastCamera = PixelartManager.Instance.GetPixelartCastCamera(camera);
            return pixelartCastCamera == null ? null : pixelartCastCamera.Camera;
        }

        #region Topdown Camera
        public static TopdownCameraController GetTopdownCameraController(Camera camera)
        {
            var resultCamera = GetResultCamera(camera);
            return TopdownCameraController.Get(resultCamera);
        }

        public static TopdownCameraController GetTopdownCameraController(PixelartCamera pixelartCamera)
        {
            return TopdownCameraController.Get(pixelartCamera.Camera);
        }

        public static TopdownCameraController GetTopdownCameraController(PixelartCastCamera pixelartCastCamera)
        {
            return TopdownCameraController.Get(pixelartCastCamera.ParentCamera.Camera);
        }
        #endregion
    }
}