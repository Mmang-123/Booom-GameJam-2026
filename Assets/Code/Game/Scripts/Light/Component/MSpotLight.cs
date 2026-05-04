using UnityEngine;

namespace Mmang.PixelartRender
{
    public class MSpotPointLight : MLight
    {
        #region IMLight
        public override Color LightColor { get => Color; set => Color = value; }
        public override float LightIntensity { get => Intensity; set => Intensity = value; }
        public override float LightRadius { get => Radius * ScaleFactor; set => Radius = value; }
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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 pos = transform.position;
            Vector3 up = transform.up;

            float scaledRadius = LightRadius;
            // Outer cone
            Gizmos.color = new Color(Color.r, Color.g, Color.b, 1f);
            Vector3 outerLeft  = Quaternion.Euler(0, 0, -OuterSpotAngle * 0.5f) * up * scaledRadius;
            Vector3 outerRight = Quaternion.Euler(0, 0,  OuterSpotAngle * 0.5f) * up * scaledRadius;
            Gizmos.DrawLine(pos, pos + outerLeft);
            Gizmos.DrawLine(pos, pos + outerRight);
            DrawArc(pos, up, OuterSpotAngle, scaledRadius);

            // Inner cone
            Gizmos.color = new Color(Color.r, Color.g, Color.b, 0.4f);
            Vector3 innerLeft  = Quaternion.Euler(0, 0, -InnerSpotAngle * 0.5f) * up * scaledRadius;
            Vector3 innerRight = Quaternion.Euler(0, 0,  InnerSpotAngle * 0.5f) * up * scaledRadius;
            Gizmos.DrawLine(pos, pos + innerLeft);
            Gizmos.DrawLine(pos, pos + innerRight);
            DrawArc(pos, up, InnerSpotAngle, scaledRadius);
        }

        private static void DrawArc(Vector3 center, Vector3 direction, float angle, float radius, int segments = 24)
        {
            float startAngle = -angle * 0.5f;
            float step = angle / segments;
            Vector3 prev = center + Quaternion.Euler(0, 0, startAngle) * direction * radius;
            for (int i = 1; i <= segments; i++)
            {
                Vector3 next = center + Quaternion.Euler(0, 0, startAngle + step * i) * direction * radius;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
#endif
    }
}