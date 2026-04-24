using UnityEngine.Rendering;

namespace Mmang.PixelartRender.VolumeComponents
{
    [System.Serializable, VolumeComponentMenu("Pixelart/SpecularHalo")]
    public class SpecularHalo : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter Intensity = new(0.5f, 0f, 1f, false);

        public bool IsActive()
        {
            return active && Intensity != 0f;
        }
    }
}