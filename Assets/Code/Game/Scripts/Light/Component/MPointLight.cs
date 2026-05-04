using UnityEngine;

namespace Mmang.PixelartRender
{
    public class MPointLight : MLight
    {
        #region IMLight
        public override Color LightColor { get => Color; set => Color = value; }
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
    }
}