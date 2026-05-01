using Mmang.PixelartRender;
using UnityEngine;

namespace Game
{
    public class ShroomlightShell : MonoBehaviour
    {
        [SerializeField] private bool m_Active;
        [SerializeField] private MLight m_Light;
        [SerializeField] private SpriteRenderer m_IndicatorRenderer;
        [SerializeField] private SpriteRenderer m_SourceRenderer;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private float m_LightIntensity = 1f;
        
        private float ActiveTime => 0.1f;
        private float IntensityUpdateRate => m_LightIntensity / 1.0f;


        private float m_ActiveTimer = 0f;
        public bool Active => m_Active;

        private void Start()
        {
            m_Light.LightIntensity = m_Active ? m_LightIntensity : 0f;
            m_IndicatorRenderer.color = m_Active ? Color.green : Color.red;
        }

        private void FixedUpdate()
        {
            if (!m_Active && LightingTextureManager.Instance.InValidChunk(transform.position))
            {
                bool lightExist = CheckLightStrength();
                if (lightExist)
                {
                    m_ActiveTimer = Mathf.Clamp(m_ActiveTimer + Time.fixedDeltaTime, 0f, ActiveTime);
                    if (m_ActiveTimer >= ActiveTime)
                    {
                        SetActive(true);
                    }
                }
                else
                {
                    m_ActiveTimer = 0f;
                }
            }

            if (m_Active && m_Light.LightIntensity < m_LightIntensity)
            {
                m_Light.LightIntensity = Mathf.Min(m_Light.LightIntensity + IntensityUpdateRate * Time.fixedDeltaTime, m_LightIntensity);
            }
            else if (!m_Active && m_Light.LightIntensity > 0f)
            {
                m_Light.LightIntensity = Mathf.Max(m_Light.LightIntensity - IntensityUpdateRate * Time.fixedDeltaTime, 0f);
            }
        }

        private void SetActive(bool active)
        {
            if (active == m_Active)
                return;
            
            m_Active = active;
            m_IndicatorRenderer.color = m_Active ? Color.green : Color.red;

            if (m_Active && m_Animator != null)
            {
                m_Animator.SetTrigger("Lit");
            }
        }

        private bool CheckLightStrength()
        {
            float strength = LightingTextureManager.Instance.GetLightStrength(transform.position);
            return strength >= 0.01f;
        }
    }
}