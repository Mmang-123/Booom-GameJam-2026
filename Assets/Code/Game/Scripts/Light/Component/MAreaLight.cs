using UnityEngine;

namespace Mmang.PixelartRender
{
    public class MAreaLight : MLight
    {
        #region IMLight
        public override float LightIntensity { get => Intensity; set => Intensity = value; }
        public override float LightRadius { get => Radius; set => Radius = value; }
        #endregion

        public Color Color = Color.white;
        public float Radius = 1f;
        public float Intensity = 1f;

        public float Width = 1f;
        public float InnerWidth = 0.5f;

        public Vector3 Position => transform.position;
        public Vector2 GetDirection()
        {
            return transform.up;
        }
        public Vector4 GetPoints()
        {
            Vector2 point1 = transform.position - Width * 0.5f * transform.right;
            Vector2 point2 = transform.position + Width * 0.5f * transform.right;
            return new(point1.x, point1.y, point2.x, point2.y);
        }
        public float GetInnerScale()
        {
            return Mathf.Clamp01(InnerWidth / Width);
        }
    }
}