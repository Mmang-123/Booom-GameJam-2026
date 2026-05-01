using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class AdditionalVelocity : IReference
    {
        public Vector2 Value;
        public float Damping;

        public static AdditionalVelocity Create(Vector2 value, float damping = 5f)
        {
            var instance = ReferencePool.Acquire<AdditionalVelocity>();
            instance.Value = value;
            instance.Damping = damping;
            return instance;
        }

        public void Clear()
        {
            Value = Vector2.zero;
            Damping = 0f;
        }
    }

    public class FB_Swim : FishBehaviour
    {
        public enum State { Normal, Trace }

        [SerializeField] private float m_RotateSpeed = 10f;
        [SerializeField] private float m_FastRotateSpeed = 10f;
        [SerializeField] private float m_MoveSpeed = 2f;
        [SerializeField] private float m_Acceleration = 2f;
        [SerializeField] private float m_StopDistance;

        // Runtime
        public State CurrentState { get; private set; }
        public bool Tracing { get; set; }
        public Vector2 TargetPoint { get; set; }
        public float CurrentSpeed { get; set; }
        public float MaxSpeed => m_MoveSpeed;

        private List<AdditionalVelocity> m_AddtionalVelocities = new();

        private void Update()
        {
            UpdateState();

            RotateToTarget();
        }

        public override void BeforeFishFixedUpdate()
        {
            switch (CurrentState)
            {
                case State.Normal:
                    NormalUpdate(Time.fixedDeltaTime);
                    break;
                case State.Trace:
                    TraceUpdate(Time.fixedDeltaTime);
                    break;
            }

            //
            HandleAdditionalVelocity(Time.fixedDeltaTime);
        }

        private void UpdateState()
        {
            // 大概是管理当前帧的状态机
            CurrentState = Tracing ? State.Trace : State.Normal;
        }

        private bool RequireFastRotate(float currentAngle)
        {
            return currentAngle >= 60f;
        }

        private void RotateToTarget()
        {
            float offsetAngle = 0f;
            switch (Fish.EDirection)
            {
                case EDirection.Up:
                    offsetAngle = -90f;
                    break;
                case EDirection.Down:
                    offsetAngle = 90f;
                    break;
                case EDirection.Left:
                    offsetAngle = 180f;
                    break;
            }

            float angle = Vector2.Angle(Fish.ForwardDirection, (TargetPoint - (Vector2)transform.position).normalized);
            float rotateSpeed = RequireFastRotate(angle) ? m_FastRotateSpeed : m_RotateSpeed;

            Vector2 direction = TargetPoint - (Vector2)transform.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + offsetAngle;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            Fish.SetRotation(Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed));
        }

        private void TraceUpdate(float dt)
        {
            float distance = Vector2.Distance(transform.position, TargetPoint);
            float angle = Vector2.Angle(Fish.ForwardDirection, (TargetPoint - (Vector2)transform.position).normalized);

            if (RequireFastRotate(angle) && CurrentSpeed > 0.1f)
            {
                var additionalVelocity = AdditionalVelocity.Create(Fish.ForwardDirection * CurrentSpeed, CurrentSpeed / 0.5f);
                AddAdditionalVelocity(additionalVelocity);
                CurrentSpeed = 0f;
            }

            if (distance > m_StopDistance && !RequireFastRotate(angle))
            {
                CurrentSpeed = Mathf.Min(m_MoveSpeed, CurrentSpeed + dt * m_Acceleration);
            }
            else
            {
                CurrentSpeed = Mathf.Max(0f, CurrentSpeed - dt * m_Acceleration * 2f);
            }

            float moveDistance = CurrentSpeed * dt;
            moveDistance = Mathf.Min(moveDistance, distance - 0.05f);

            Vector2 motion = moveDistance * Fish.ForwardDirection;
            //transform.position += (Vector3)motion;
            Fish.Move(motion);
        }

        private void NormalUpdate(float dt)
        {
            if (CurrentSpeed > 0f)
            {
                CurrentSpeed = Mathf.Max(0f, CurrentSpeed - dt * m_Acceleration * 2f);
                float moveDistance = CurrentSpeed * dt;

                Vector2 motion = moveDistance * Fish.ForwardDirection;
                //transform.position += (Vector3)motion;
                Fish.Move(motion);
            }
        }

        private void HandleAdditionalVelocity(float dt)
        {
            var toRemove = ListPool<AdditionalVelocity>.Get();            
            foreach (var velocity in m_AddtionalVelocities)
            {
                //
                Fish.Move(dt * velocity.Value);       

                //
                Vector2 direction = velocity.Value.normalized;
                float length = velocity.Value.magnitude;
                length -= dt * velocity.Damping;
                if (length <= 0f)
                {
                    toRemove.Add(velocity);
                    ReferencePool.Release(velocity);
                }
                else
                {
                    velocity.Value = length * direction;
                }
            }

            m_AddtionalVelocities.RemoveAll(i => toRemove.Contains(i));
            
            ListPool<AdditionalVelocity>.Release(toRemove);
        }

        public void AddAdditionalVelocity(AdditionalVelocity velocity)
        {
            m_AddtionalVelocities.Add(velocity);
        }

        public void ClearAdditionalVelocity()
        {
            foreach (var velocity in m_AddtionalVelocities)
            {
                ReferencePool.Release(velocity);
            }
            m_AddtionalVelocities.Clear();
        }

    }
}