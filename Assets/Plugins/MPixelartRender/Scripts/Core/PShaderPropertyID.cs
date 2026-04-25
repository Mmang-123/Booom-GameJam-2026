
using UnityEngine;

namespace Mmang.PixelartRender
{
    public static class PShaderPropertyID
    {
        // 相机矩阵
        public static readonly int ViewMatrix = Shader.PropertyToID("unity_MatrixV");
        public static readonly int InvViewMatrix = Shader.PropertyToID("unity_MatrixInvV");
        public static readonly int CameraViewMatrix = Shader.PropertyToID("_CameraMatrixV");
        public static readonly int CameraInvViewMatrix = Shader.PropertyToID("_CameraMatrixInvV");
        public static readonly int CameraViewProjectionMatrix = Shader.PropertyToID("_CameraMatrixVP");
        public static readonly int CameraInvViewProjectionMatrix = Shader.PropertyToID("_CameraMatrixInvVP");

        // 相机
        public static readonly int Resolution = Shader.PropertyToID("_Resolution");
        public static readonly int UnitSize = Shader.PropertyToID("_UnitSize");
        public static readonly int CameraScale = Shader.PropertyToID("_CameraScale");

        // 光照
        public static readonly int AdditionalLightCount = Shader.PropertyToID("_AdditionalLightCount");
    
        // Player Data
        public static readonly int CenterFocusPosition = Shader.PropertyToID("_CenterFocusPosition");

        // Texture
        public static readonly int PixelartLUT = Shader.PropertyToID("_PixelartLUT");
    
        // Cloud
        public static readonly int CloudTexture = Shader.PropertyToID("_CloudTexture");
        public static readonly int CloudSize = Shader.PropertyToID("_CloudSize");

        // Lighting
        public static readonly int LightingTexture = Shader.PropertyToID("_LightingTexture");
    }
}