using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FA_TraceBack : FishAIAbility
    {
        public List<Vector2> TraceBackPoints = new();

        //
        private FB_Swim m_SwimBehaviour;
        private Vector2 m_PrePoint;

        protected override void OnInit()
        {
            base.OnInit();
            m_SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
        }

        public override bool CanActivateAbility()
        {
            return TraceBackPoints.Count > 0;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            m_SwimBehaviour.Tracing = true;
        }

        public override void OnEnd(EEndAbilityType endType)
        {
            base.OnEnd(endType);
            m_SwimBehaviour.Tracing = false;
        }

        public override void OnUpdate(float dt)
        {
            if (TraceBackPoints.Count <= 0)
            {
                FishAI.PendingEndAbility(this, EEndAbilityType.End);
                return;
            }

            
            Vector2 targetPoint = TraceBackPoints[^1];
            m_SwimBehaviour.TargetPoint = targetPoint;

            float distance = Vector2.Distance(targetPoint, Fish.Position);
            if (distance < 1f)
            {
                TraceBackPoints.RemoveAt(TraceBackPoints.Count - 1);
                return;
            }
        }

        public void UpdateTracePath(Vector2 newPoint, bool init)
        {
            if (TraceBackPoints.Count == 0 || init)
            {
                m_PrePoint = newPoint;
                TraceBackPoints.Add(newPoint);
                return;
            }

            Vector2 curPoint = TraceBackPoints[^1];
            if (FishUtils.RaycastObstacle(curPoint, newPoint))
            {
                TraceBackPoints.Add(m_PrePoint);
            }
            m_PrePoint = newPoint;

            {
                Vector2 point = TraceBackPoints[0];
                Vector2 nextPoint;
                for (int i = 0; i < TraceBackPoints.Count; i++)
                {
                    nextPoint = TraceBackPoints[i];
                    Debug.DrawLine(point, nextPoint, Color.red);
                    point = nextPoint;
                }

                Debug.DrawLine(point, Fish.Position, Color.red);
            }
        }
    }
}