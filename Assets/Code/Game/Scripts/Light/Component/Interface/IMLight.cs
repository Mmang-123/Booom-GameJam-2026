
using UnityEngine;

namespace Game
{
    public interface IMLight
    {
        public Color LightColor { get; set; }
        public float LightIntensity { get; set; }
        public float LightRadius { get; set; }

        public Bounds GetBounds();
    }
}