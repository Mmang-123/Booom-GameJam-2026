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
        public enum State { Normal, Trace, Disable }

        [Header("基础设置")]
        [SerializeField] private float m_RotateSpeed = 3f;
        [SerializeField] private float m_FastRotateSpeed = 6f;
        [SerializeField] private float m_MoveSpeed = 10f;
        [SerializeField] private float m_Acceleration = 5f;
        [SerializeField] private float m_FastAcceleration = 16f;
        [SerializeField] private float m_StopDistance;

        [Header("避障")]
        [SerializeField] private bool m_CanAvoidance = false;
        [SerializeField] private float m_RayDistance = 2f;    // 射线检测距离
        [SerializeField] private float m_RayAngle = 30f;      // 左右射线的角度
        //[SerializeField] private float avoidanceForce = 3f; // 避障时的排斥力倍数
        [SerializeField] private LayerMask m_ObstacleLayer;   // 障碍物图层


        // Runtime
        public State CurrentState { get; private set; }
        public bool Tracing { get; set; }
        public Vector2 TargetPoint { get; set; }
        public Vector2 TargetDirection { get; set; }
        public float CurrentSpeed { get; set; }
        public bool RotateToTargetPoint { get; set; }
        public bool IsDisable { get; set; }

        //
        public float AdditionalSpeed { get; set; } = 0f;
        public float AdditionalRotateSpeed { get; set; } = 0f;

        public float MaxSpeed => m_MoveSpeed;
        public float StopDistance => m_StopDistance;

        private List<AdditionalVelocity> m_AddtionalVelocities = new();

        public bool CanAvoidance { get => m_CanAvoidance; set => m_CanAvoidance = value; }

        private void Update()
        {
            UpdateState();

            if (RotateToTargetPoint || Tracing)
            {
                Vector2 targetDirection = (TargetPoint - (Vector2)transform.position).normalized;
                if (m_CanAvoidance && TryAvoidance(targetDirection, out var avoidanceVector))
                {
                    //var q = Quaternion.Lerp(Quaternion.LookRotation(targetDirection), Quaternion.LookRotation(avoidanceVector), 0.5f);
                    //Vector2 vec = (q * Vector3.forward).GetXY().normalized;
                    Vector2 vec = avoidanceVector.normalized;
                    TargetDirection = vec;
                }
                else
                {
                    TargetDirection = targetDirection;
                }
            }

            if (!IsDisable)
            {
                RotateToTarget();
            }
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
                default:
                    DefaultUpdate(Time.fixedDeltaTime);
                    break;
            }

            //
            HandleAdditionalVelocity(Time.fixedDeltaTime);
        }

        private void UpdateState()
        {
            // 大概是管理当前帧的状态机
            if (IsDisable)
                CurrentState = State.Disable;
            else
                CurrentState = Tracing ? State.Trace : State.Normal;
        }

        private bool RequireFastRotate(float currentAngle)
        {
            return currentAngle >= 60f;
        }

        private void RotateToTarget()
        {
            float angle = Vector2.Angle(Fish.ForwardDirection, TargetDirection);
            float rotateSpeed = RequireFastRotate(angle) ? m_FastRotateSpeed : m_RotateSpeed;
            rotateSpeed += AdditionalRotateSpeed;

            Quaternion targetRotation = Fish.GetRotation(TargetDirection);
            Fish.SetRotation(Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed));
        }

        #region Update

        private void TraceUpdate(float dt)
        {
            float distance = Vector2.Distance(transform.position, TargetPoint);
            float angle = Vector2.Angle(Fish.ForwardDirection, TargetDirection);

            if (RequireFastRotate(angle) && CurrentSpeed > 0.1f)
            {
                var additionalVelocity = AdditionalVelocity.Create(Fish.ForwardDirection * CurrentSpeed, CurrentSpeed / 0.5f);
                AddAdditionalVelocity(additionalVelocity);
                CurrentSpeed = 0f;
            }

            float acceleration = RequireFastRotate(angle) ? m_FastAcceleration : m_Acceleration;

            if (distance > m_StopDistance && !RequireFastRotate(angle))
            {
                CurrentSpeed = Mathf.Min(m_MoveSpeed, CurrentSpeed + dt * acceleration);
            }
            else
            {
                CurrentSpeed = Mathf.Max(0f, CurrentSpeed - dt * acceleration * 2f);
            }

            float speed = CurrentSpeed + AdditionalSpeed;
            float moveDistance = speed * dt;
            moveDistance = Mathf.Min(moveDistance, Mathf.Max(0f, distance - m_StopDistance / 4f));
            //moveDistance = Mathf.Min(moveDistance, Mathf.Max(0f, distance - 0.005f));

            Vector2 motion = moveDistance * Fish.ForwardDirection;
            Fish.Move(motion);
        }

        private void NormalUpdate(float dt)
        {
            if (CurrentSpeed > 0f)
            {
                CurrentSpeed = Mathf.Max(0f, CurrentSpeed - dt * m_Acceleration * 2f);
                float moveDistance = CurrentSpeed * dt;

                Vector2 motion = moveDistance * Fish.ForwardDirection;
                Fish.Move(motion);
            }
        }

        private void DefaultUpdate(float dt)
        {
            float moveDistance = CurrentSpeed * dt;

            Vector2 motion = moveDistance * Fish.ForwardDirection;
            Fish.Move(motion);
        }

        #endregion


        #region 避障

        private bool TryAvoidance(Vector2 targetDirection, out Vector2 avoidanceDirection)
        {
            Vector2 forward = targetDirection;
            Vector2 leftRayDir = Quaternion.Euler(0, 0, m_RayAngle) * forward;
            Vector2 rightRayDir = Quaternion.Euler(0, 0, -m_RayAngle) * forward;

            /*
            RaycastHit2D hitCenter = Physics2D.Raycast(transform.position, forward, m_RayDistance, m_ObstacleLayer);
            RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, leftRayDir, m_RayDistance, m_ObstacleLayer);
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position, rightRayDir, m_RayDistance, m_ObstacleLayer);
            */

            RaycastHit2D hitCenter = FishUtils.RaycastObstacle(Fish.Position, forward, m_RayDistance);
            RaycastHit2D hitLeft = FishUtils.RaycastObstacle(Fish.Position, leftRayDir, m_RayDistance);
            RaycastHit2D hitRight = FishUtils.RaycastObstacle(Fish.Position, rightRayDir, m_RayDistance);

            // Debug 画线
            Debug.DrawRay(transform.position, forward * m_RayDistance, Color.green);
            Debug.DrawRay(transform.position, leftRayDir * m_RayDistance, Color.yellow);
            Debug.DrawRay(transform.position, rightRayDir * m_RayDistance, Color.yellow);

            int turnDir = 0;
            Vector2 hitNormal = Vector2.zero;

            if (hitCenter)
            {
                if (Vector3.Cross(targetDirection, Fish.ForwardDirection).z > 0f)
                    turnDir = 1; // 向左
                else
                    turnDir = -1;
                
                hitNormal = hitCenter.normal;
                //avoidanceDirection = (avoidanceTurnDir == 1f ? Quaternion.Euler(0, 0, 70f) : Quaternion.Euler(0, 0, -70f)) * -hitCenter.normal;
                //Debug.DrawRay(transform.position, avoidanceDirection, Color.red);
            }
            else if (hitLeft && hitRight)
            {
                if (hitLeft.distance <= hitRight.distance)
                {
                    turnDir = -1;
                    hitNormal = hitLeft.normal;
                    Debug.Log(hitLeft.collider);
                }
                else
                {
                    turnDir = 1;
                    hitNormal = hitRight.normal;
                    Debug.Log(hitRight.collider);
                }
            }
            else if (hitLeft)
            {
                turnDir = -1;
                hitNormal = hitLeft.normal;
            }
            else if (hitRight)
            {
                turnDir = 1;
                hitNormal = hitRight.normal;
            }

            if (turnDir != 0)
            {
                avoidanceDirection = (turnDir == 1f ? Quaternion.Euler(0, 0, 90f) : Quaternion.Euler(0, 0, -90f)) * -hitNormal;

                //RaycastHit2D hit = Physics2D.Raycast(transform.position, avoidanceDirection, m_RayDistance, m_ObstacleLayer);
                RaycastHit2D hit = FishUtils.RaycastObstacle(Fish.Position, avoidanceDirection.normalized, m_RayDistance);
                if (hit)
                {
                    avoidanceDirection = (turnDir == 1f ? Quaternion.Euler(0, 0, 45f) : Quaternion.Euler(0, 0, -45f)) * avoidanceDirection;
                }

                Debug.DrawRay(transform.position, avoidanceDirection, Color.red);
                return true;
            }

            /*
            if (hitLeft)
            {
                avoidanceDirection = Quaternion.Euler(0, 0, -70f) * -hitLeft.normal;
                Debug.DrawRay(transform.position, avoidanceDirection, Color.red);
                return true;
            }
            if (hitRight)
            {
                avoidanceDirection = Quaternion.Euler(0, 0, 70f) * -hitRight.normal;
                Debug.DrawRay(transform.position, avoidanceDirection, Color.red);
                return true;
            }
            */


            avoidanceDirection = Vector2.zero;
            return false;
        }

        #endregion


        #region Additional Velocity

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

        #endregion

    }
}