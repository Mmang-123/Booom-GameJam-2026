using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FA_Patrol : FishAIAbility
    {
        public enum ELoopType
        {
            Normal, PingPong
        }

        [System.Serializable]
        public struct PatrolPoint
        {
            public Transform Trans;
            public float WaitTime;
            public readonly Vector2 Position => Trans.position;
        }

        [SerializeField] private ELoopType m_LoopType = ELoopType.Normal;
        [SerializeField] private List<PatrolPoint> m_PatrolPoints = new();

        // Runtime
        public int CurrentIndex { get; private set; }
        public bool IsReverse { get; private set; }
        public float WaitTimer { get; private set; }
        private FB_Swim SwimBehaviour { get; set; }

        public override void OnActivate()
        {
            FindNearestPoint(out int index, out bool reverse);
            CurrentIndex = index;
            IsReverse = reverse;
            WaitTimer = 0f;
            SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
            SwimBehaviour.RotateToTargetPoint = false;
        }

        public override void OnEnd(EEndAbilityType endType)
        {
            SwimBehaviour.Tracing = false;
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            if (WaitTimer <= 0f && CurrentIndex != -1)
            {
                SwimBehaviour.TargetPoint = GetPointPosition(CurrentIndex);
                SwimBehaviour.Tracing = true;

                if (GetDistanceToPoint(CurrentIndex) <= SwimBehaviour.StopDistance + 0.05f)
                {
                    StartMoveToPoint(LoopIndex(), false);
                }

            }
            else if (WaitTimer > 0f)
            {
                WaitTimer -= dt;
            }

        }

        private void StartMoveToPoint(int index, bool immediate = false)
        {
            SwimBehaviour.Tracing = false;
            if (CurrentIndex != -1 && !immediate)
            {
                WaitTimer = m_PatrolPoints[CurrentIndex].WaitTime;
            }
            else
            {
                WaitTimer = 0f;
            }

            CurrentIndex = index;
        }

        private int LoopIndex()
        {
            if (m_LoopType == ELoopType.PingPong)
            {
                if (CurrentIndex == m_PatrolPoints.Count - 1)
                {
                    IsReverse = true;
                }
                else if (CurrentIndex == 0)
                {
                    IsReverse = false;
                }
            }

            return GetNextIndex(CurrentIndex, IsReverse);
        }

        #region

        private Vector2 GetPointPosition(int index)
        {
            return m_PatrolPoints[index].Position;
        }

        private int GetNextIndex(int index, bool reverse = false)
        {
            int count = m_PatrolPoints.Count;
            index += reverse ? -1 : 1;
            if (index <= -1)
                index = count - 1;
            else if (index >= count)
                index = 0;
            return index;
        }

        private Vector2 GetDirectionToPoint(int index)
        {
            return (m_PatrolPoints[index].Position - Fish.Position).normalized;
        }

        private float GetDistanceToPoint(int index)
        {
            return Vector2.Distance(Fish.Position, m_PatrolPoints[index].Position);
        }

        #endregion

        private void FindNearestPoint(out int outIndex, out bool outReverse)
        {
            int index = -1;
            float nearestDistance = float.MaxValue;
            
            for (int i = 0; i < m_PatrolPoints.Count; i++)
            {
                float distance = GetDistanceToPoint(i);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    index = i;
                }
            }

            if (index != -1 && m_PatrolPoints.Count > 1 && m_LoopType == ELoopType.PingPong)
            {
                int nextIndex1 = GetNextIndex(index, false);
                int nextIndex2 = GetNextIndex(index, true);

                Vector2 direction = GetDirectionToPoint(index);
                Vector2 direction1 = GetDirectionToPoint(nextIndex1);
                Vector2 direction2 = GetDirectionToPoint(nextIndex2);

                outReverse = Vector2.Dot(direction, direction1) >= Vector2.Dot(direction, direction2);
            }
            else
            {
                outReverse = true;
            }

            outIndex = index;
        }
    }
}