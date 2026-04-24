using UnityEngine;

namespace Game
{
    public class FB_Swim : FishBehaviour
    {
        public enum State { Normal, Trace }

        [SerializeField] private float m_RotateSpeed = 10f;
        [SerializeField] private float m_MoveSpeed = 2f;
        [SerializeField] private float m_Acceleration = 2f;
        [SerializeField] private float m_StopDistance;

        // Runtime
        public State CurrentState { get; private set; }
        public bool Tracing { get; set; }
        public Vector2 TargetPoint { get; set; }
        public float CurrentSpeed { get; private set; }

        private void Update()
        {
            UpdateState();

            RotateToTarget();

            switch (CurrentState)
            {
                case State.Normal:
                    NormalUpdate();
                    break;
                case State.Trace:
                    TraceUpdate();
                    break;
            }
        }

        private void UpdateState()
        {
            // 大概是管理当前帧的状态机
            CurrentState = Tracing ? State.Trace : State.Normal;
        }

        private void RotateToTarget()
        {
            Vector2 direction = TargetPoint - (Vector2)transform.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * m_RotateSpeed);
        }

        private void TraceUpdate()
        {
            float distance = Vector2.Distance(transform.position, TargetPoint);
            if (distance > m_StopDistance)
            {
                CurrentSpeed = Mathf.Min(m_MoveSpeed, CurrentSpeed + Time.deltaTime * m_Acceleration);
            }
            else
            {
                CurrentSpeed = Mathf.Max(0f, CurrentSpeed - Time.deltaTime * m_Acceleration * 2f);
            }

            float moveDistance = CurrentSpeed * Time.deltaTime;
            moveDistance = Mathf.Min(moveDistance, distance - 0.05f);

            Vector2 motion = moveDistance * transform.up;
            transform.position += (Vector3)motion;
        }

        private void NormalUpdate()
        {
            if (CurrentSpeed > 0f)
            {
                CurrentSpeed = Mathf.Max(0f, CurrentSpeed - Time.deltaTime * m_Acceleration * 2f);
                float moveDistance = CurrentSpeed * Time.deltaTime;

                Vector2 motion = moveDistance * transform.up;
                transform.position += (Vector3)motion;
            }
        }

    }
}