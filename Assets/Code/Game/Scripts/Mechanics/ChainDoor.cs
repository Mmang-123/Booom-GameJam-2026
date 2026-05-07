using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [ExecuteAlways]
    public class ChainDoor : MonoBehaviour, IChargable
    {
        [SerializeField] private float m_DoorLength = 3f;
        [SerializeField] private int m_RequirePowerSourceCount = 1;

        [SerializeField] private BoxCollider2D m_BoxCollider;
        [SerializeField] private SpriteRenderer m_DoorRenderer;
        [SerializeField] private List<SpriteRenderer> m_Indicators;
        [SerializeField] private Color m_ActiveColor = Color.green;
        [SerializeField] private Color m_InactiveColor = Color.red;

        [Header("音效")]
        [SerializeField] private AudioClipRef m_OpenStartClip;
        [SerializeField] private AudioClipRef m_OpenCompleteClip;
        [SerializeField] private AudioClipRef m_CloseStartClip;
        [SerializeField] private AudioClipRef m_CloseCompleteClip;

        private bool m_Active;
        private float m_CurrentDoorLength;
        private bool m_IsInitializing = true;
        private AudioSource m_CurrentStartSource;
        private AudioSource m_CurrentCompleteSource;

        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered(m_RequirePowerSourceCount);

        #endregion

#if UNITY_EDITOR
        private float m_OldDoorLength;
#endif

        private void Start()
        {
            SetDoorLength(m_DoorLength);
            m_IsInitializing = false;
        }

        private void FixedUpdate()
        {
            CheckSlots();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (m_OldDoorLength != m_DoorLength)
                {
                    m_OldDoorLength = m_DoorLength;
                    SetDoorLength(m_DoorLength);
                }
                return;
            }
#endif

            if (m_Active && m_CurrentDoorLength > 0f)
            {
                m_CurrentDoorLength -= Time.deltaTime * 6f;
                m_CurrentDoorLength = Mathf.Max(0f, m_CurrentDoorLength);
                SetDoorLength(m_CurrentDoorLength);

                if (m_CurrentDoorLength == 0f)
                    m_CurrentCompleteSource = AudioManager.PlayManaged(m_OpenCompleteClip, transform.position);
            }
            else if (!m_Active && m_CurrentDoorLength < m_DoorLength)
            {
                m_CurrentDoorLength += Time.deltaTime * 6f;
                m_CurrentDoorLength = Mathf.Min(m_DoorLength, m_CurrentDoorLength);
                SetDoorLength(m_CurrentDoorLength);

                if (m_CurrentDoorLength == m_DoorLength)
                    m_CurrentCompleteSource = AudioManager.PlayManaged(m_CloseCompleteClip, transform.position);
            }
        }

        private void CheckSlots()
        {
            bool powerOn = true;
            for (int i = 0; i < m_RequirePowerSourceCount; i++)
            {
                bool slotActive = PowerSourceHandler.GetSlotActive(i);
                var indicator = m_Indicators[i];
                if (indicator != null)
                {
                    indicator.color = slotActive ? m_ActiveColor : m_InactiveColor;
                }

                powerOn &= slotActive;
            }

            if (powerOn != m_Active)
                SetActive(powerOn);
        }

        private void SetActive(bool active, bool playSFX = true)
        {
            bool changed = (m_Active != active);
            m_Active = active;
            //m_Emission.color = active ? m_ActiveColor : m_InactiveColor;

            if (!m_IsInitializing && playSFX && changed)
            {
                AudioManager.StopManaged(ref m_CurrentStartSource);
                AudioManager.StopManaged(ref m_CurrentCompleteSource);
                m_CurrentStartSource = AudioManager.PlayManaged(active ? m_OpenStartClip : m_CloseStartClip, transform.position);
            }
        }

        private void SetDoorLength(float newLength)
        {
            Vector2 pos = new(-0.5f - newLength / 2f, 0f);

            if (m_DoorRenderer != null)
            {
                m_DoorRenderer.size = new(newLength, 1f);
                m_DoorRenderer.transform.localPosition = pos;
                m_CurrentDoorLength = newLength;   
            }

            if (m_BoxCollider != null)
            {
                m_BoxCollider.size = new(newLength, m_BoxCollider.size.y);
                m_BoxCollider.offset = pos;
            }
        }

        public void SetChargeComplete(bool init)
        {
            SetActive(true, false);
            SetDoorLength(0f);
        }
    }
}