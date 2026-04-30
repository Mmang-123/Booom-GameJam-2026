using System.Collections.Generic;
using Mmang.Generic;
using UnityEngine;

namespace Game
{
    public class LightDetector : MonoBehaviour, IPowerSource
    {
        [SerializeField] private SpriteRenderer m_EmissionLight;
        
        private float ActiveTime => 0.1f;
        private float MaxActiveTime => ActiveTime * 2f;

        public bool Active => m_Active;
        public bool PowerOn => Active;
        public event System.Action<bool> OnPowerChanged;

        // Runtime
        private float m_ActiveTimer;
        private bool m_Active;

        private void Start()
        {
            m_Active = false;
        }

        private void FixedUpdate()
        {
            bool lightExist = CheckLightStrength();
            m_ActiveTimer = Mathf.Clamp(m_ActiveTimer + Time.fixedDeltaTime * (lightExist ? 1 : -1), 0f, MaxActiveTime);
            SetActive(m_ActiveTimer >= ActiveTime);
        }

        private bool CheckLightStrength()
        {
            float strength = LightingTextureManager.Instance.GetLightStrength(transform.position);
            return strength >= 0.01f;
        }

        private void SetActive(bool active)
        {
            if (m_Active == active)
                return;

            m_Active = active;
            OnActiveChanged();
        }

        private void OnActiveChanged()
        {
            m_EmissionLight.color = m_Active ? Color.green : Color.red;
            OnPowerChanged?.Invoke(m_Active);
        }
    }
}