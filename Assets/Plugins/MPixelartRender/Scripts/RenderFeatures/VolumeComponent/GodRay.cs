using UnityEngine;
using UnityEngine.Rendering;

namespace Mmang.PixelartRender.VolumeComponents
{
    [System.Serializable, VolumeComponentMenu("Pixelart/GodRay")]
    public class GodRay : VolumeComponent, IPostProcessComponent
    {
        [Header("GodRay")]
        public ClampedFloatParameter Intensity = new(0.5f, 0f, 1f);
        public ColorParameter Color = new(new Color(1, 1, 1));
        public FloatParameter GradientBottom = new(0.5f);
        public FloatParameter GradientHeight = new(12f);

        [Header("Quad")]
        public ClampedIntParameter QuadCount = new(18, 0, 128);
        public Vector2Parameter QuadSpacingRange = new(new Vector2(-30f, 30f));
        public ClampedFloatParameter QuadAlpha = new(0.02f, 0f, 1f);

        public bool IsActive()
        {
            return active && Intensity != 0f && QuadCount.value > 0
            && QuadSpacingRange.value.x < QuadSpacingRange.value.y;
        }
    }
}