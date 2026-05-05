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
        [SerializeField] private float m_DarkSightRadius = 3f;
        [SerializeField] private Transform m_CheckLightPoint;

        // Runtime
        public float CD { get; private set; }
        private float m_IntensityT = 0f;
        private float m_DarkSightRadiusT;

        private float IntensityUpdateRate => 3f;

        public override bool CanUse()
        {
            return CD <= 0f;
        }

        public override void Use()
        {
            m_Active = !m_Active;
            CD = m_CD;
            
            if (Fish.IsPlayer)
            {
                var darkSightManager = DarkSightManager.Instance;
                if (!m_Active)
                {
                    darkSightManager.SetOverrideByLight(true);
                }
            }
        }

        private void Start()
        {
            m_IntensityT = m_Active ? 1f : 0f;
            m_SpotLight.LightIntensity = Mathf.SmoothStep(0f, m_SpotLightIntensity, m_IntensityT);
            m_PointLight.LightIntensity = Mathf.SmoothStep(0f, m_PointLightIntensity, m_IntensityT);
        }

        private void FixedUpdate()
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

            if (Fish.IsPlayer)
            {
                var darkSightManager = DarkSightManager.Instance;
                bool requireDarkSight = !m_Active && !CheckLightStrength();

                if (!requireDarkSight && m_DarkSightRadiusT > 0f)
                {
                    m_DarkSightRadiusT = Mathf.Clamp01(m_DarkSightRadiusT - IntensityUpdateRate * Time.fixedDeltaTime);
                    darkSightManager.SetRadius(Mathf.SmoothStep(0f, m_DarkSightRadius, m_DarkSightRadiusT));
                }
                else if (requireDarkSight && m_DarkSightRadiusT < 1f)
                {
                    m_DarkSightRadiusT = Mathf.Clamp01(m_DarkSightRadiusT + IntensityUpdateRate * Time.fixedDeltaTime);
                    darkSightManager.SetRadius(Mathf.SmoothStep(0f, m_DarkSightRadius, m_DarkSightRadiusT));
                }

                darkSightManager.SetCenterPosition(Fish.Position);
            }
        }

        private bool CheckLightStrength()
        {
            if (m_CheckLightPoint == null)
                return false;
            float strength = LightingTextureManager.Instance.GetLightStrength(m_CheckLightPoint.transform.position);
            return strength >= 0.01f;
        }
    }
}