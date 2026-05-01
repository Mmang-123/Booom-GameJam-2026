using UnityEngine;

namespace Mmang.PixelartRender
{
    [System.Serializable]
    public struct LightData2D
    {
        public const int SIZE = (4 + 4 + 4 + 3) * sizeof(float);
        public Vector4 position; // xy: 位置, z: 内圈半径, w: 半径
        public Vector4 color;    // rgb: 颜色, w: 强度

        // 对于聚光: 1.xy: 方向, 1.z: scale, 1.w: offset
        // 对于面光: 1.xy: 方向, 1.zw: 端点1, 2.xy: 端点2, 2.z: innerScale
        public Vector4 lightParams1;
        public Vector3 lightParams2;
    }
}