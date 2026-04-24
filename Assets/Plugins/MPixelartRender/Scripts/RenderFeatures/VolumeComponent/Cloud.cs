using UnityEngine.Rendering;

namespace Mmang.PixelartRender.VolumeComponents
{
    [System.Serializable, VolumeComponentMenu("Pixelart/Cloud")]
    public class Cloud : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter Enable = new(true, true);

        public bool IsActive()
        {
            return active && Enable.value;
        }
    }
}