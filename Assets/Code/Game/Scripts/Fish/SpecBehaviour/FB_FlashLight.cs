using Mmang.PixelartRender;
using UnityEngine;

namespace Game
{
    public class FB_FlashLight : FB_Skill
    {
        [SerializeField] private bool m_Active;
        [SerializeField] private float m_CD = 0.5f;
        [SerializeField] private MLight m_SpotLight;
        [SerializeField] private MLight m_PointLight;
        [SerializeField] private float m_SpotLightIntensity = 1f;
        [SerializeField] private float m_PointLightIntensity = 1f;

        // Runtime
        public float CD { get; private set; }
        private float m_IntensityT = 0f;

        private float IntensityUpdateRate => 3f;

        public override bool CanUse()
        {
            return CD <= 0f;
        }

        public override void Use()
        {
            m_Active = !m_Active;
            CD = m_CD;
        }

        private void Start()
        {
            m_IntensityT = m_Active ? 1f : 0f;
            m_SpotLight.LightIntensity = Mathf.SmoothStep(0f, m_SpotLightIntensity, m_IntensityT);
            m_PointLight.LightIntensity = Mathf.SmoothStep(0f, m_PointLightIntensity, m_IntensityT);
        }

        private void Update()
        {
            if (CD > 0f)
            {
                CD -= Time.deltaTime;
            }

            if (m_Active && m_SpotLight.LightIntensity < m_SpotLightIntensity)
            {
                m_IntensityT = Mathf.Clamp01(m_IntensityT + IntensityUpdateRate * Time.fixedDeltaTime);
                m_SpotLight.LightIntensity = Mathf.SmoothStep(0f, m_SpotLightIntensity, m_IntensityT);
                m_PointLight.LightIntensity = Mathf.SmoothStep(0f, m_PointLightIntensity, m_IntensityT);
            }
            else if (!m_Active && m_SpotLight.LightIntensity > 0f)
            {
                m_IntensityT = Mathf.Clamp01(m_IntensityT - IntensityUpdateRate * Time.fixedDeltaTime);
                m_SpotLight.LightIntensity = Mathf.SmoothStep(0f, m_SpotLightIntensity, m_IntensityT);
                m_PointLight.LightIntensity = Mathf.SmoothStep(0f, m_PointLightIntensity, m_IntensityT);
            }
        }
    }
}