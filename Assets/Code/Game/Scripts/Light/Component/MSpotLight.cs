using UnityEngine;

namespace Mmang.PixelartRender
{
    public class MSpotPointLight : MLight
    {
        #region IMLight
        public override float LightIntensity { get => Intensity; set => Intensity = value; }
        public override float LightRadius { get => Radius; set => Radius = value; }
        #endregion
  
        public Color Color = Color.white;
        public float Radius = 1f;
        public float InnerRadius = 0.3f;
        public float Intensity = 1f;
        public float InnerSpotAngle;       // 内锥角（度）
        public float OuterSpotAngle;       // 外锥角（度）

        public Vector3 Position => transform.position;
    
        public Vector2 GetScaleOffset()
        {
            float innerCos = Mathf.Cos(InnerSpotAngle * Mathf.Deg2Rad * 0.5f);
            float outerCos = Mathf.Cos(OuterSpotAngle * Mathf.Deg2Rad * 0.5f);
            float angleScale = 1.0f / Mathf.Max(innerCos - outerCos, 0.001f);
            float angleOffset = -outerCos * angleScale;
            return new(angleScale, angleOffset);
        }

        public Vector2 GetDirection()
        {
            return transform.up;
        }

        public override Bounds GetBounds()
        {
            Bounds bounds = new()
            {
                center = transform.position,
                extents = new(LightRadius, LightRadius)
            };

            return bounds;
        }
    }
}