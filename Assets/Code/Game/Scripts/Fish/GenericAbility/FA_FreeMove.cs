using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Game
{
    public class FA_FreeMove : FishAIAbility
    {
        [SerializeField] private CircleCollider2D m_Range;
        [SerializeField] private Vector2 m_WaitTimeRange;

        // Runtime
        private Vector2 m_RangeCenter;
        private float m_RangeRadius;
        private bool m_Moving;
        private float m_WaitTimer;
        private Vector2 m_TargetPosition;

        private FB_Swim SwimBehaviour { get; set; }

        protected override void OnInit()
        {
            SetRange((Vector2)m_Range.transform.position + m_Range.offset, m_Range.radius);
        }

        public override bool CanActivateAbility()
        {
            return true;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
            Wait();
        }

        public override void OnUpdate(float dt)
        {
            if (m_Moving)
            {
                SwimBehaviour.TargetPoint = m_TargetPosition;
                if (Vector2.Distance(Fish.Position, m_TargetPosition) <= 1.6f)
                {
                    Wait();
                    return;
                }
            }
            else
            {
                if (m_WaitTimer > 0f)
                {
                    m_WaitTimer -= dt;
                }
                else
                {
                    MoveToRandomPoint();
                }
            }
        }
       
        private void MoveToRandomPoint()
        {
            m_Moving = true;
            m_TargetPosition = GetRandomPointInCircle();
            SwimBehaviour.Tracing = true;
            SwimBehaviour.TargetPoint = m_TargetPosition;
            SwimBehaviour.RotateToTargetPoint = false;
        }

        private void Wait()
        {
            m_Moving = false;
            m_WaitTimer = RandomUtil.GetRandomValueInRange(m_WaitTimeRange);
            SwimBehaviour.Tracing = false;
        }

        public void SetRange(Vector2 center, float radius)
        {
            m_RangeCenter = center;
            m_RangeRadius = radius;
        }

        private Vector2 GetRandomPointInCircle()
        {
            return Random.insideUnitCircle * m_RangeRadius + m_RangeCenter;
        }
    }
}