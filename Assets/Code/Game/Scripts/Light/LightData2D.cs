using UnityEngine;

namespace Mmang.PixelartRender
{
    [System.Serializable]
    public struct LightData2D
    {
        public const int SIZE = (4 + 4 + 4) * sizeof(float);
        public Vector4 position; // xyz: 位置, w: 半径
        public Vector4 color;    // rgb: 颜色, w: 强度
        public Vector4 spotLightParams; // xy: 方向, z: scale, w: offset
    }
}