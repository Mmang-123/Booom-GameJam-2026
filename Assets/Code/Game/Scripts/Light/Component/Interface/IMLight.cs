
namespace Game
{
    public interface IMLight
    {
        public float LightIntensity { get; set; }
        public float LightRadius { get; set; }

        // todo: 包围盒
    }
}