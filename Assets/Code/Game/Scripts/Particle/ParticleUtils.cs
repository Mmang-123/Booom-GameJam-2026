using Mmang.Util;
using UnityEngine;

namespace Game
{
    public static class ParticleUtils
    {
        #region Play
        public static void Play(ParticleSystem prefab, Vector2 position)
            => Play(prefab, position);
        public static void Play(ParticleSystem prefab, Vector2 position, Quaternion rotation)
        {
            if (prefab.TryGetComponent<ParticleComponent>(out var component))
            {
                Play(component, position, rotation);
            }
        }

        public static void Play(string particleName, Vector2 position)
            => Play(particleName, position, Quaternion.identity);
        public static void Play(string particleName, Vector2 position, Quaternion rotation)
        {
            var prefab = ParticleConfig.GetParticle(particleName);
            if (prefab != null)
                Play(prefab, position, rotation);
        }

        public static void Play(ParticleComponent prefab, Vector2 position)
            => Play(prefab, position, Quaternion.identity);
        public static void Play(ParticleComponent prefab, Vector2 position, Quaternion rotation)
        {
            var instance = GlobalGameObjectPool.Get(prefab.ParticleName, position, rotation, prefab);
            instance.StartPlay();
        }

        #endregion



        #region Create

        public static ParticleComponent Create(ParticleSystem prefab, Vector2 position)
            => Create(prefab, position);
        public static ParticleComponent Create(ParticleSystem prefab, Vector2 position, Quaternion rotation)
        {
            if (prefab.TryGetComponent<ParticleComponent>(out var component))
            {
                return Create(component, position, rotation);
            }
            return null;
        }

        public static ParticleComponent Create(string particleName, Vector2 position)
            => Create(particleName, position, Quaternion.identity);
        public static ParticleComponent Create(string particleName, Vector2 position, Quaternion rotation)
        {
            var prefab = ParticleConfig.GetParticle(particleName);
            if (prefab != null)
                return Create(prefab, position, rotation);
            return null;
        }

        public static ParticleComponent Create(ParticleComponent prefab, Vector2 position)
            => Create(prefab, position, Quaternion.identity);
        public static ParticleComponent Create(ParticleComponent prefab, Vector2 position, Quaternion rotation)
        {
            return GlobalGameObjectPool.Get(prefab.ParticleName, position, rotation, prefab);;
        }

        #endregion
    }
}