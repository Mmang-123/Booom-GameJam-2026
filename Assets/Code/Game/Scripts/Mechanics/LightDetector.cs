using System.Collections.Generic;
using Mmang.Generic;
using UnityEngine;

namespace Game
{
    public class LightDetector : MonoBehaviour, IPowerSource
    {
        [SerializeField] private SpriteRenderer m_EmissionLight;
        [SerializeField] private bool m_InitTurnOn = false;

        private float ActiveTime => 0.1f;
        private float MaxActiveTime => ActiveTime * 2f;

        public bool Active => m_Active;
        public bool PowerOn => Active;
        public event System.Action<bool> OnPowerChanged;

        public Color ActiveColor = Color.green;
        public Color InactiveColor = Color.red;

        // Runtime
        private float m_ActiveTimer;
        private bool m_Active;

        private void Start()
        {
            m_Active = m_InitTurnOn;
            if (m_Active)
            {
                
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
            m_EmissionLight.color = m_Active ? ActiveColor : InactiveColor;
            OnPowerChanged?.Invoke(m_Active);
        }
    }
}