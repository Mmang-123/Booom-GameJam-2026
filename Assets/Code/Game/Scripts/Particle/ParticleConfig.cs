using UnityEngine;
using Mmang;
using System.Collections.Generic;

namespace Game
{
    [MGlobalConfig(configName = "Particle Config")]
    public class ParticleConfig : ScriptableObject
    {
        [SerializeField] private List<ParticleComponent> m_Data = new();

        // Runtime
        [System.NonSerialized] private bool m_Inited = false;
        [System.NonSerialized] private Dictionary<string, ParticleComponent> m_Map;

        private void Init()
        {
            if (m_Inited)
                return;
            m_Inited = true;

            m_Map ??= new();
            m_Map.Clear();

            foreach (var component in m_Data)
            {
                if (m_Map.ContainsKey(component.ParticleName))
                    continue;
                m_Map.Add(component.ParticleName, component);
            }
        }

        public static ParticleComponent GetParticle(string particleName)
        {
            var instance = GlobalConfigAssets.GetConfigInstance<ParticleConfig>();
            instance.Init();
            
            if (instance.m_Map.TryGetValue(particleName, out var result))
                return result;
            return null;
        }
    }
}