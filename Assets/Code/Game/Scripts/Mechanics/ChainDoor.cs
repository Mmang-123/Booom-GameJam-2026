using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ChainDoor : ChargableMono
    {
        [SerializeField] private float m_DoorLength = 3f;
        [SerializeField] private int m_RequirePowerSourceCount = 1;

        [SerializeField] private SpriteRenderer m_DoorRenderer;
        [SerializeField] private SpriteRenderer m_Emission;

        private int m_PowerSourceCount = 0;
        private bool m_Active;
        private float m_CurrentDoorLength;

        private void Start()
        {
            SetDoorLength(m_DoorLength);
        }

        private void Update()
        {
            if (m_Active && m_CurrentDoorLength > 0f)
            {
                m_CurrentDoorLength -= Time.deltaTime * 6f;
                m_CurrentDoorLength = Mathf.Max(0f, m_CurrentDoorLength);
                SetDoorLength(m_CurrentDoorLength);
            }
            else if (!m_Active && m_CurrentDoorLength < m_DoorLength)
            {
                m_CurrentDoorLength += Time.deltaTime * 6f;
                m_CurrentDoorLength = Mathf.Min(m_DoorLength, m_CurrentDoorLength);
                SetDoorLength(m_CurrentDoorLength);
            }
        }

        public override void StartCharge(IPowerSource powerSource)
        {
            m_PowerSourceCount++;
            if (m_PowerSourceCount >= m_RequirePowerSourceCount)
                SetActive(true);
        }

        public override void StopCharge(IPowerSource powerSource)
        {
            m_PowerSourceCount--;
            if (m_PowerSourceCount < m_RequirePowerSourceCount)
                SetActive(false);
        }

        private void SetActive(bool active)
        {
            m_Active = active;
            m_Emission.color = active ? Color.green : Color.red;
        }

        private void SetDoorLength(float newLength)
        {
            m_DoorRenderer.size = new(newLength, 1f);
            m_DoorRenderer.transform.localPosition = new(-0.5f - newLength / 2f, 0f);
            m_CurrentDoorLength = newLength;
        }

        private void OnValidate()
        {
            SetDoorLength(m_DoorLength);
        }
    }
}