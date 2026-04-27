using UnityEngine;

namespace Mmang.PixelartRender
{
    public class MAreaLight : MLight
    {
        public Color Color = Color.white;
        public float Radius = 1f;
        public float Intensity = 1f;

        public Vector3 Position => transform.position;
    }
}