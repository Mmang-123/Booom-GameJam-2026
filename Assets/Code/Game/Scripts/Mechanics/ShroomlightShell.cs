using Mmang.PixelartRender;
using UnityEngine;

namespace Game
{
    public class ShroomlightShell : MonoBehaviour, IChargable
    {
        [SerializeField] private MLight m_Light;
        [SerializeField] private SpriteRenderer m_IndicatorRenderer;
        [SerializeField] private SpriteRenderer m_SourceRenderer;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private float m_LightIntensity = 1f;
        [SerializeField] private ParticleSystem m_Particle;

        [Header("音效")]
        [SerializeField] private AudioClipRef m_LitClip;
        [SerializeField] private AudioClipRef m_UnlitClip;
        
        private float IntensityUpdateRate => 1.0f / 1.0f;

        private bool m_Active;
        private float m_IntensityT = 0f; // [0,1] 用于 smoothstep

        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered();

        #endregion

        //private float m_ActiveTimer = 0f;
        //public bool Active => m_Active;

        private void Start()
        {
            m_IndicatorRenderer.color = m_Active ? Color.green : Color.red;
        }

        private void FixedUpdate()
        {
            SetActive(IsPowered);

            if (m_Active && m_Light.LightIntensity < m_LightIntensity)
            {
                m_IntensityT = Mathf.Clamp01(m_IntensityT + IntensityUpdateRate * Time.fixedDeltaTime);
                m_Light.LightIntensity = Mathf.SmoothStep(0f, m_LightIntensity, m_IntensityT);
            }
            else if (!m_Active && m_Light.LightIntensity > 0f)
            {
                m_IntensityT = Mathf.Clamp01(m_IntensityT - IntensityUpdateRate * Time.fixedDeltaTime);
                m_Light.LightIntensity = Mathf.SmoothStep(0f, m_LightIntensity, m_IntensityT);
            }
        }

        private void SetActive(bool active)
        {
            if (active == m_Active)
                return;
            
            m_Active = active;
            m_IndicatorRenderer.color = m_Active ? Color.green : Color.red;

            if (m_Animator != null)
                m_Animator.SetTrigger(m_Active ? "Lit" : "Unlit");
            
            if (m_Particle != null)
            {
                if (m_Active)
                    m_Particle.Play();
                else
                    m_Particle.Stop();
            }

            AudioManager.PlayAtPosition(m_Active ? m_LitClip : m_UnlitClip, transform.position);
        }

        public void SetChargeComplete(bool init)
        {
            if (init)
            {
                m_Active = true;
                if (m_Animator != null)
                {
                    m_Animator.SetTrigger("InitLit");
                }

                if (m_Particle != null)
                {
                    m_Particle.Play();
                }
            }
            else
            {
                m_Active = false;
                SetActive(true);
            }

            m_IntensityT = 1f;
            m_Light.LightIntensity = m_LightIntensity;
            m_IndicatorRenderer.color = Color.green;
        }

    }
}