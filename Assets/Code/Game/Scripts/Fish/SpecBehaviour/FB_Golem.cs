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
        [SerializeField] private List<Transform> m_CheckPoints = new();

        // Runtime
        private bool m_Active;
        private List<IGolemBehaviour> m_GolemBehaviours = new();
        private float m_ActiveTimer = 0f;

        private float ActiveTime => 0.1f;
        private float MaxActiveTime => 0.8f;

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
        }

        private void FixedUpdate()
        {
            if (LightingTextureManager.Instance.InValidChunk(transform.position))
            {
                bool lightExist = CheckLightStrength();
                m_ActiveTimer = Mathf.Clamp(m_ActiveTimer + Time.fixedDeltaTime * (lightExist ? 1 : -1), 0f, MaxActiveTime);
                SetActive(m_ActiveTimer >= ActiveTime);
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

        private bool CheckLightStrength()
        {
            float strength = 0f;
            foreach (var point in m_CheckPoints)
            {
                strength += LightingTextureManager.Instance.GetLightStrength(point.position);
            }

            return strength >= 0.0625f;
        }
    }
}