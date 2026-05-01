using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ChainDoor : MonoBehaviour, IChargable
    {
        [SerializeField] private float m_DoorLength = 3f;
        [SerializeField] private int m_RequirePowerSourceCount = 1;

        [SerializeField] private SpriteRenderer m_DoorRenderer;
        [SerializeField] private SpriteRenderer m_Emission;
        [SerializeField] private Color m_ActiveColor = Color.green;
        [SerializeField] private Color m_InactiveColor = Color.red;

        private bool m_Active;
        private float m_CurrentDoorLength;

        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered(m_RequirePowerSourceCount);

        #endregion

        private void Start()
        {
            SetDoorLength(m_DoorLength);
        }

        private void FixedUpdate()
        {
            if (IsPowered != m_Active)
            {
                SetActive(IsPowered);
            }
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

        private void SetActive(bool active)
        {
            m_Active = active;
            m_Emission.color = active ? m_ActiveColor : m_InactiveColor;
        }

        private void SetDoorLength(float newLength)
        {
            m_DoorRenderer.size = new(newLength, 1f);
            m_DoorRenderer.transform.localPosition = new(-0.5f - newLength / 2f, 0f);
            m_CurrentDoorLength = newLength;
        }
    }
}