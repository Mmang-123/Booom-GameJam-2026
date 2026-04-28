
using UnityEngine;

namespace Game
{
    public class LightDetector : MonoBehaviour, IPowerSource
    {
        [SerializeField] private ChargableMono m_ChargeObject;
        [SerializeField] private SpriteRenderer m_EmissionLight;
        private bool m_Active;

        private void Start()
        {
            m_Active = false;
        }

        private void FixedUpdate()
        {
            float strength = LightingTextureManager.Instance.GetLightStrength(transform.position);
            if (strength <= 0.01f)
            {
                SetActive(false);
            }
            else
            {
                SetActive(true);
            }
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
            if (m_ChargeObject != null)
            {
                if (m_Active)
                {
                    m_ChargeObject.StartCharge(this);
                }
                else
                {
                    m_ChargeObject.StopCharge(this);
                }   
            }
        }
    }
}