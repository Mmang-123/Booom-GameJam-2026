using UnityEngine;

namespace Mmang.PixelartRender
{
    public class MAreaLight : MLight
    {
        public Color Color = Color.white;
        public float Radius = 1f;
        public float Intensity = 1f;

        public float width = 1f;

        public Vector3 Position => transform.position;
        public Vector2 GetDirection()
        {
            return transform.up;
        }
        public Vector4 GetPoints()
        {
            Vector2 point1 = transform.position - width * 0.5f * transform.right;
            Vector2 point2 = transform.position + width * 0.5f * transform.right;
            return new(point1.x, point1.y, point2.x, point2.y);
        }
    }
}