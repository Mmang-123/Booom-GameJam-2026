using UnityEngine;

namespace Game
{
    public class LoopZone : MonoBehaviour
    {
        public enum EState { Negative, Include, Positive }

        [SerializeField] private BoxCollider2D m_Zone;
    
        // Runtime
        private bool m_Active;
        private Vector2 m_Center;
        private Vector2 m_RightDirection;
        private Vector2 m_UpDirection;
        private Vector2 m_ZoneSize;

        private void Start()
        {
            m_Center = (Vector2)transform.position + m_Zone.offset;
            m_ZoneSize = m_Zone.size;

            m_RightDirection = m_Zone.transform.right;
            m_UpDirection = m_Zone.transform.up;
        }

        private void FixedUpdate()
        {
            var player = PlayerController.Instance;
            var fish = player != null ? player.Fish : null;
            if (fish == null)
                return;

            if (!m_Active)
            {
                var states = CheckPosition(fish.Position);
                if (states.horizontalState == EState.Include && states.verticalState == EState.Include)
                {
                    SetActive(true);
                }
            }
            else
            {
                Vector2 offset = fish.Position - m_Center;
                float xLength = Vector2.Dot(offset, m_RightDirection);
                float yLength = Vector2.Dot(offset, m_UpDirection);
                
                Vector2 newPosition = fish.Position;
                bool changed = false;

                if (xLength > m_ZoneSize.x * 0.5f)
                {
                    newPosition -= m_RightDirection * m_ZoneSize.x;
                    changed = true;
                }
                else if (xLength < -m_ZoneSize.x * 0.5f)
                {
                    newPosition += m_RightDirection * m_ZoneSize.x;
                    changed = true;
                }

                if (yLength > m_ZoneSize.y * 0.5f)
                {
                    newPosition -= m_UpDirection * m_ZoneSize.y;
                    changed = true;
                }
                else if (yLength < -m_ZoneSize.y * 0.5f)
                {
                    newPosition += m_UpDirection * m_ZoneSize.y;
                    changed = true;
                }

                if (changed)
                    player.Transfer(newPosition);
            }
        }

        private void SetActive(bool active)
        {
            m_Active = active;
            Debug.Log("Loop: " + active);
        }

        private (EState horizontalState, EState verticalState) CheckPosition(Vector2 position)
        {
            (EState horizontalState, EState verticalState) result = new();

            Vector2 offset = position - m_Center;
            float xLength = Vector2.Dot(offset, m_RightDirection) * 2f;
            float yLength = Vector2.Dot(offset, m_UpDirection) * 2f;

            if (Mathf.Abs(xLength) <= m_ZoneSize.x)
                result.horizontalState = EState.Include;
            else if (xLength > 0f)
                result.horizontalState = EState.Positive;
            else
                result.horizontalState = EState.Negative;
            
            if (Mathf.Abs(yLength) <= m_ZoneSize.y)
                result.verticalState = EState.Include;
            else if (yLength > 0f)
                result.verticalState = EState.Positive;
            else
                result.verticalState = EState.Negative;

            return result;
        }
    
    }
}