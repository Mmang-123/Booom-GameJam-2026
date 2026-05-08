using UnityEngine;

namespace Game
{
    public class LoopZone : MonoBehaviour
    {
        public enum EState { Negative, Include, Positive }

        [SerializeField] private BoxCollider2D m_Zone;
    
        // Runtime
        [SerializeField] private bool m_Active;
        private Vector2 m_Center;
        private Vector2 m_RightDirection;
        private Vector2 m_UpDirection;
        private Vector2 m_ZoneSize;

        public delegate bool CheckDelegate((EState horizontalState, EState verticalState) states);
        public CheckDelegate CheckFunc;

        public bool Active => m_Active;

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

            if (m_Active)
            {
                Vector2 offset = fish.Position - m_Center;
                float xLength = Vector2.Dot(offset, m_RightDirection);
                float yLength = Vector2.Dot(offset, m_UpDirection);
                
                Vector2 newPosition = fish.Position;
                bool changed = false;

                (EState horizontalState, EState verticalState) states = new()
                {
                    horizontalState = EState.Include,
                    verticalState = EState.Include
                };

                if (xLength > m_ZoneSize.x * 0.5f)
                {
                    newPosition -= m_RightDirection * m_ZoneSize.x;
                    changed = true;
                    states.horizontalState = EState.Positive;
                }
                else if (xLength < -m_ZoneSize.x * 0.5f)
                {
                    newPosition += m_RightDirection * m_ZoneSize.x;
                    changed = true;
                    states.horizontalState = EState.Negative;
                }

                if (yLength > m_ZoneSize.y * 0.5f)
                {
                    newPosition -= m_UpDirection * m_ZoneSize.y;
                    changed = true;
                    states.verticalState = EState.Positive;
                }
                else if (yLength < -m_ZoneSize.y * 0.5f)
                {
                    newPosition += m_UpDirection * m_ZoneSize.y;
                    changed = true;
                    states.verticalState = EState.Negative;
                }

                if (changed && (CheckFunc == null || CheckFunc(states)))
                    player.Transfer(newPosition);
            }
        }

        public void SetActive(bool active)
        {
            m_Active = active;
        }

        public (EState horizontalState, EState verticalState) CheckPosition(Vector2 position)
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