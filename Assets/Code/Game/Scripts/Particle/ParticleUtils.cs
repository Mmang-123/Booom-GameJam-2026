using UnityEngine;

namespace Game
{
    public static class ParticleUtils
    {
        public static void Play(ParticleSystem prefab)
        {
            if (prefab.TryGetComponent<ParticleComponent>(out var component))
            {
                
            }
        }
    }
}