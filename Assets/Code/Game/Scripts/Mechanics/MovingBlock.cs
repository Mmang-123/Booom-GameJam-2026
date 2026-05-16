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
        [SerializeField] private float m_MoveBackTime = 0.5f;

        [SerializeField] private LayerMask m_ObstacleLayer = ~0;
        [SerializeField] private BoxCollider2D m_Box;
        [SerializeField] private SpriteRenderer m_BoxRenderer;
        [SerializeField] private SpriteRenderer m_ChainRenderer;
        [SerializeField] private SpriteRenderer m_ChainEndRenderer;
        [SerializeField] private SpriteRenderer m_Emission;
        [SerializeField] private AudioClipRef m_MoveClip;
        [SerializeField] private AudioClipRef m_HitClip;

        public float StartDistance => 0.5f + 0.0625f + m_BoxSize.x / 2f;
        public float EndDistance => StartDistance + m_ChainLength - m_BoxSize.x - 0.125f;

        #region IChargable
        public PowerSourceHandler PowerSourceHandler { get; } = new();
        public bool IsPowered => PowerSourceHandler.IsPowered();

        #endregion

        // Runtime
        private float m_T;
        private bool m_Active;
        private bool m_IsMoving;
        private bool m_HitPlayed;


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

            bool wasMoving = m_IsMoving;
            m_IsMoving = false;

            if (!PowerSourceHandler.IsValid())
            {
                return;
            }

            if (!m_Active && IsPowered)
            {
                SetActive(true);
            }

            if (m_Active)
            {
                if (m_T < 1f)
                {
                    //m_T = Mathf.Clamp01(m_T + Time.deltaTime / m_MoveTime);
                    //MoveBox(m_T);
                    float nextT = Mathf.Clamp01(m_T + Time.deltaTime / m_MoveTime);
                    
                    // 只有在没有物理阻挡的情况下才实际更新 m_T 和位置
                    if (CanMove(m_T, nextT))
                    {
                        m_T = nextT;
                        MoveBox(m_T);
                    }
                    else
                    {
                        PlayHitSound();
                        if (!IsPowered)
                            SetActive(false);
                    }
                }
                else if (!IsPowered)
                {
                    SetActive(false);
                }
            }
            else if (m_T > 0f)
            {
                // 预测回退时的下一步 T 值
                float nextT = Mathf.Clamp01(m_T - Time.deltaTime / m_MoveBackTime);
                
                // 回退时同样检测物理阻挡（如果确定回退时不会有障碍物，可以把这个检测去掉）
                if (CanMove(m_T, nextT))
                {
                    m_T = nextT;
                    MoveBox(m_T);
                }
                else
                {
                    PlayHitSound();
                    if (IsPowered)
                        SetActive(true);
                }
            }

            // 移动音效：开始移动时按速度设置 pitch 播放一次
            if (!wasMoving && m_IsMoving)
            {
                m_HitPlayed = false;
                float moveTime = m_Active ? m_MoveTime : m_MoveBackTime;
                float pitch = 1f / Mathf.Max(moveTime, 0.05f);
                AudioManager.PlayAtPosition(m_MoveClip, transform.position, pitch: pitch);
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

        private void PlayHitSound()
        {
            if (m_HitPlayed) return;
            m_HitPlayed = true;
            AudioManager.PlayAtPosition(m_HitClip, transform.position);
        }

        /// <summary>
        /// 检测从当前的 T 移动到下一个 T 是否会撞到障碍物
        /// </summary>
        private static RaycastHit2D[] s_Results = new RaycastHit2D[16];
        private bool CanMove(float currentT, float nextT)
        {
            // 按照原逻辑计算当前的实际距离和下一步的实际距离
            float currentEased = Mathf.SmoothStep(0f, 1f, currentT);
            float nextEased = Mathf.SmoothStep(0f, 1f, nextT);

            float currentDist = Mathf.Lerp(StartDistance, EndDistance, m_Reverse ? (1 - currentEased) : currentEased);
            float nextDist = Mathf.Lerp(StartDistance, EndDistance, m_Reverse ? (1 - nextEased) : nextEased);

            // 计算这一帧将会移动的距离量
            float deltaDist = nextDist - currentDist;

            // 如果这一帧几乎不移动，直接允许
            if (Mathf.Abs(deltaDist) <= 0.0001f) return true;

            // 确定检测的方向
            Vector2 direction = deltaDist > 0 ? transform.right : -transform.right;
            float distance = Mathf.Abs(deltaDist);
            Vector2 castSize = m_BoxSize * 0.9f;

            // ================= 新增：使用 ContactFilter2D 忽略 Trigger =================
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;          // 核心：强制忽略 isTrigger = true 的碰撞体
            filter.SetLayerMask(m_ObstacleLayer); // 设置需要检测的 Layer
            filter.useLayerMask = true;          // 启用 LayerMask 过滤

            // 发射 BoxCast，使用 filter 进行过滤
            int hitCount = Physics2D.BoxCast(
                m_Box.transform.position, 
                castSize, 
                m_Box.transform.eulerAngles.z, 
                direction, 
                filter, 
                s_Results, 
                distance
            );

            for (int i = 0; i < hitCount; i++)
            {
                if (s_Results[i].collider == m_Box)
                    continue;
                return false;
            }

            return true;
            // 如果 hitCount 为 0，说明前方没有 非Trigger 的障碍物，可以继续移动
            //return hitCount == 0;
        }

        private void MoveBox(float t)
        {
            m_IsMoving = true;
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