using UnityEngine;

namespace Mmang.PixelartRender
{
    public class MPointLight : MLight
    {
        #region IMLight
        public override float LightIntensity { get => Intensity; set => Intensity = value; }
        public override float LightRadius { get => Radius; set => Radius = value; }
        #endregion

        public Color Color = Color.white;
        public float Radius = 1f;
        public float InnerRadius = 0.3f;
        public float Intensity = 1f;

        public Vector3 Position => transform.position;

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
            // Hard cutoff boundary (raw Radius) — dim reference
            UnityEditor.Handles.color = new Color(Color.r, Color.g, Color.b, 0.2f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, Radius);

            // Effective visible boundary derived from shader step-quantization:
            //   visible when: intensity * (1 - dist/radius)^2 >= 0.25
            //   => dist <= radius * (1 - 0.5 / sqrt(intensity))
            float effectiveRadius = Radius * Mathf.Max(0f, 1f - 0.5f / Mathf.Sqrt(Mathf.Max(Intensity, 0.0001f)));
            UnityEditor.Handles.color = new Color(Color.r, Color.g, Color.b, 1f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, effectiveRadius);
        }
#endif
    }
}