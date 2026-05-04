using UnityEngine;

namespace Mmang.PixelartRender
{
    public class MAreaLight : MLight
    {
        #region IMLight
        public override Color LightColor { get => Color; set => Color = value; }
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

        public override Bounds GetBounds()
        {
            float len = Mathf.Max(Width, Radius) * 1.42f;
            Bounds bounds = new()
            {
                center = (Vector2)transform.position + GetDirection() * Radius / 2f,
                extents = new(len, len)
            };

            return bounds;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 pos   = transform.position;
            Vector3 right = transform.right;
            Vector3 up    = transform.up;

            // Light source line — outer width
            Gizmos.color = new Color(Color.r, Color.g, Color.b, 1f);
            Vector3 p1 = pos - right * Width * 0.5f;
            Vector3 p2 = pos + right * Width * 0.5f;
            Gizmos.DrawLine(p1, p2);

            // Inner width
            Gizmos.color = new Color(Color.r, Color.g, Color.b, 0.4f);
            Gizmos.DrawLine(pos - right * InnerWidth * 0.5f, pos + right * InnerWidth * 0.5f);

            // Reach (Radius) — side walls + far edge
            Gizmos.color = new Color(Color.r, Color.g, Color.b, 0.7f);
            Vector3 p1Far = p1 + up * Radius;
            Vector3 p2Far = p2 + up * Radius;
            Gizmos.DrawLine(p1, p1Far);
            Gizmos.DrawLine(p2, p2Far);
            Gizmos.DrawLine(p1Far, p2Far);
        }
#endif
    }
}