
using UnityEngine;

namespace Game
{
    public interface IMLight
    {
        public float LightIntensity { get; set; }
        public float LightRadius { get; set; }

        public Bounds GetBounds();
    }
}