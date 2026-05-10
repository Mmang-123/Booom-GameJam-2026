using UnityEngine;
using System.Collections.Generic;

namespace Game
{

    public interface IGolemBehaviour
    {
        public void SetGolemActive(bool active);
    }

    public class FB_Golem : FishBehaviour
    {
        [SerializeField] private bool m_InitActive = false;
        [SerializeField] private SpriteRenderer m_EyeRenderer;
        [SerializeField] private float m_EyeFadeDuration = 1.5f;
        [SerializeField] private List<Transform> m_CheckPoints = new();

        // Runtime
        private bool m_Active;
        private List<IGolemBehaviour> m_GolemBehaviours = new();
        private float m_ActiveTimer = 0f;
        private float m_EyeActiveTimer = 0f;

        private float ActiveTime => 0.05f;
        private float MaxActiveTime => 2.0f;

        public bool Active => m_Active;


        private void Start()
        {
            m_GolemBehaviours.Clear();
            foreach (var behaviour in Fish.Behaviours)
            {
                if (behaviour is IGolemBehaviour golemBehaviour)
                {
                    m_GolemBehaviours.Add(golemBehaviour);
                }
            }

            if (m_InitActive)
            {
                m_Active = false;
                SetActive(true);
                m_ActiveTimer = MaxActiveTime;
            }

            SetEyeStrength(m_Active ? 1f : 0f);
        }

        private void FixedUpdate()
        {
            if (LightingTextureManager.Instance.InValidChunk(transform.position))
            {
                {
                    // 石化
                    bool lightExist = CheckLightStrength();
                    m_ActiveTimer = Mathf.Clamp(m_ActiveTimer + Time.fixedDeltaTime * (lightExist ? 1 : -1), 0f, MaxActiveTime);
                    SetActive(m_ActiveTimer >= ActiveTime);

                    // 眼睛
                    if (m_Active)
                    {
                        if (m_EyeActiveTimer < m_EyeFadeDuration)
                        {
                            m_EyeActiveTimer = Mathf.Max(0f, m_EyeActiveTimer + Time.fixedDeltaTime * 8f);
                            SetEyeStrength(m_EyeActiveTimer / m_EyeFadeDuration);   
                        }
                    }
                    else
                    {
                        if (m_EyeActiveTimer > 0f)
                        {
                            m_EyeActiveTimer = Mathf.Max(0f, m_EyeActiveTimer - Time.fixedDeltaTime);
                            SetEyeStrength(m_EyeActiveTimer / m_EyeFadeDuration);
                        }
                    }
                }
            }

            if (Fish.IsPlayer)
            {
                var darkSightManager = DarkSightManager.Instance;
                darkSightManager.SetOverrideByLight(false);
                darkSightManager.SetCenterPosition(Fish.Position);
                if (darkSightManager.Radius <= 24f)
                {
                    darkSightManager.SetRadius(darkSightManager.Radius + Time.fixedDeltaTime * 64f);
                }
            }
        }

        private void SetActive(bool active)
        {
            if (m_Active == active)
                return;

            m_Active = active;
            if (m_Active)
                m_ActiveTimer = MaxActiveTime;
            
            foreach (var behaviour in m_GolemBehaviours)
            {
                behaviour.SetGolemActive(active);
            }
        }

        private void SetEyeStrength(float t)
        {
            m_EyeRenderer.color = new Color(1, 1, 1, Mathf.SmoothStep(0f, 1f, 1f - t));
        }

        private bool CheckLightStrength()
        {
            float strength = 0f;
            foreach (var point in m_CheckPoints)
            {
                strength += LightingTextureManager.Instance.GetLightStrength(point.position);
            }

            return strength >= 0.0625 * 3.0;
        }

        private bool CheckEyeLightStrength()
        {
            float strength = 0f;
            strength += LightingTextureManager.Instance.GetLightStrength(m_CheckPoints[0].position);
            return strength >= 0.0625;
        }
    }
}