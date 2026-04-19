using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mmang.Util
{
    public static class CameraUtil
    {
        public static void CopyCameraProperties(Camera source, Camera target)
        {
            target.orthographic = source.orthographic;
            target.orthographicSize = source.orthographicSize;
            target.nearClipPlane = source.nearClipPlane;
            target.farClipPlane = source.farClipPlane;
            target.backgroundColor = source.backgroundColor;
        }
        
        public static void SimpleCopyCamera(Camera source, Camera target)
        {
            target.transform.position = source.transform.position;
            target.transform.rotation = source.transform.rotation;
            target.transform.localScale = source.transform.localScale;

            CopyCameraProperties(source, target);
        }

        public static Vector2 RestoreScreenPosition01(Vector2 position01)
        {
            return new(position01.x * Screen.width, position01.y * Screen.height);
        }
    }
}