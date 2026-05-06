using UnityEngine;

namespace Game
{
    [ExecuteAlways]
    public class MovingBlock : MonoBehaviour, IChargable
    {
        [SerializeField] private bool m_Reverse;
        [SerializeField] private Vector2 m_BoxSize = new(3f, 3f);
        [SerializeField] private float m_ChainLength = 6f;
        [SerializeField] private float m_MoveTime = 0.5f;

        [SerializeField] private BoxCollider2D m_Box;
        [SerializeField] private SpriteRenderer m_BoxRenderer;
        [SerializeField] private SpriteRenderer m_ChainRenderer;
        [SerializeField] private SpriteRenderer m_ChainEndRenderer;
        [SerializeField] private SpriteRenderer m_Emission;

        public float StartDistance => 0.5f + 0.0625f + m_BoxSize.x / 2f;
        public float EndDistance => StartDistance + m_ChainLength - m_BoxSize.x - 0.125f;

        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered();

        #endregion

        // Runtime
        private float m_T;
        private bool m_Active;


#if UNITY_EDITOR
        private float m_OldChainLength;
        private Vector2 m_OldBoxSize;
        private bool m_OldReverse;
#endif

        private void Start()
        {
            
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (m_OldChainLength != m_ChainLength)
                {
                    m_ChainLength = Mathf.Max(m_ChainLength, 3f);
                    m_OldChainLength = m_ChainLength;
                    SetChainLength(m_ChainLength);
                }
                if (m_OldReverse != m_Reverse)
                {
                    m_OldReverse = m_Reverse;
                    if (m_Box != null)
                    {
                        Vector2 pos = new(m_Reverse ? EndDistance : StartDistance, 0f);
                        m_Box.transform.localPosition = pos;   
                    }
                }
                if (m_OldBoxSize != m_BoxSize)
                {
                    m_OldBoxSize = m_BoxSize;
                    if (m_BoxRenderer != null)
                    {
                        m_BoxRenderer.size = m_BoxSize + new Vector2(0.125f, 0.125f);
                    }
                    if (m_Box != null)
                    {
                        m_Box.size = m_BoxSize;
                        Vector2 pos = new(m_Reverse ? EndDistance : StartDistance, 0f);
                        m_Box.transform.localPosition = pos;   
                    }
                }

                return;
            }
#endif

            if (!m_Active && m_T <= 0f && IsPowered)
            {
                SetActive(true);
            }

            if (m_Active)
            {
                if (m_T < 1f)
                {
                    m_T = Mathf.Clamp01(m_T + Time.deltaTime / m_MoveTime);
                    MoveBox(m_T);
                }
                else if (!IsPowered)
                {
                    SetActive(false);
                }
            }
            else if (m_T > 0f)
            {
                m_T = Mathf.Clamp01(m_T - Time.deltaTime / m_MoveTime);
                MoveBox(m_T);
            }
        }

        private void SetActive(bool active)
        {
            m_Active = active;
            if (m_Emission != null)
            {
                m_Emission.color = active ? Color.green : Color.red;
            }
        }

        private void MoveBox(float t)
        {
            t = Mathf.SmoothStep(0f, 1f, t);
            float distance = Mathf.Lerp(StartDistance, EndDistance, m_Reverse ? (1 - t) : t);
            m_Box.transform.localPosition = new(distance, 0f);
        }

        private void SetChainLength(float newLength)
        {
            m_ChainLength = newLength;

            if (m_ChainRenderer != null)
            {
                Vector2 pos = new(0.5f + newLength / 2f, 0f);
                m_ChainRenderer.size = new(newLength, 1f);
                m_ChainRenderer.transform.localPosition = pos;
            }

            if (m_ChainEndRenderer != null)
            {
                Vector2 pos = new(1f + newLength, 0f);
                m_ChainEndRenderer.transform.localPosition = pos;
            }

            if (m_Box != null)
            {
                Vector2 pos = new(m_Reverse ? EndDistance : StartDistance, 0f);
                m_Box.transform.localPosition = pos;
            }
        }

        public void SetChargeComplete(bool init)
        {
            m_Active = true;
            if (m_Emission != null)
            {
                m_Emission.color = Color.green;
            }
            MoveBox(1);
        }
    }
}