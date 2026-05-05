using Mmang.Util;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleComponent : MonoBehaviour
    {
        [Header("粒子名(不要重复)")]
        [SerializeField] private string m_ParticleName;
        public string ParticleName => m_ParticleName;

        // Runtime
        public static ParticleSystem System { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool UseOverrideColor { get; set; }
        public Color OverrideColor { get; set; }


        private void Awake()
        {
            System = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (IsPlaying)
            {
                if (!System.isPlaying)
                {
                    EndPlay();
                }
            }
        }

        public void SetOverrideColor(Color color)
        {
            UseOverrideColor = true;
            OverrideColor = color;
        }

        public void StartPlay()
        {
            if (System == null)
                return;
            
            if (UseOverrideColor)
            {
                var main = System.main;
                main.startColor = OverrideColor;
            }

            System.Play();
            IsPlaying = true;
        }

        public void EndPlay()
        {
            if (System.isPlaying)
                System.Stop();
            GlobalGameObjectPool.Release(gameObject, m_ParticleName);
        }
    }
}