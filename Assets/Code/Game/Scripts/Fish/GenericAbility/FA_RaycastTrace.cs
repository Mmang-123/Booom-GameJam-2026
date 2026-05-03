using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class FA_RaycastRangeTrace : FA_Trace
    {
        [SerializeField] private float m_SearchingRayLength = 7f;
        [SerializeField] private float m_TracingRayLength = 12f;
        [SerializeField] private int m_MaxTurningPointCount = 2;

        private Vector2 m_CurrentTurningPoint;
        private List<Vector2> m_TurningPoints = new();
        private bool m_HasPreTracingPoint;
        private Vector2 m_PreTracingPoint;

        private RaycastHit2D Raycast(Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            float distance = direction.sqrMagnitude;
            direction.Normalize();

            var hit = Physics2D.Raycast(start, direction, distance, FishUtils.WithoutFishLayer);
            return hit;
        }

        protected override bool FindTarget(out Fish outTarget)
        {
            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(Fish.Position, m_SearchingRayLength, fishList, ignoreFish: Fish, clearResultList: true);

            bool flag = false;
            outTarget = null;

            foreach (var fish in fishList)
            {
                if (!Raycast(Fish.Position, fish.Position))
                {
                    flag = true;
                    outTarget = fish;
                    break;
                }
            }

            ListPool<Fish>.Release(fishList);
            return flag;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            m_TurningPoints.Clear();

            m_HasPreTracingPoint = true;
            m_PreTracingPoint = TargetFish.Position;
        }

        public override void OnUpdate(float dt)
        {
            if (TargetFish == null)
            {
                FishAI.PendingEndAbility(this, EEndAbilityType.End);
                return;
            }

            // 如果可以直接追踪到目标，放弃所有拐点
            if (Vector2.Distance(Fish.Position, TargetFish.Position) <= m_TracingRayLength
            && !Raycast(Fish.Position, TargetFish.Position))
            {
                m_TurningPoints.Clear();
                m_PreTracingPoint = TargetFish.Position;
                m_HasPreTracingPoint = true;

                SwimBehaviour.TargetPoint = TargetFish.Position;
            }
            else
            {
                

                if (m_TurningPoints.Count < m_MaxTurningPointCount)
                {
                    UpdateTurningPoint();   
                }

                if (m_TurningPoints.Count > 0)
                {
                    // 如果可以追踪到下个拐点，放弃当前拐点
                    if (m_TurningPoints.Count > 1)
                    {
                        var nextPoint = m_TurningPoints[1];
                        if (Vector2.Distance(Fish.Position, nextPoint) <= m_TracingRayLength
                        && !Raycast(Fish.Position, nextPoint))
                        {
                            m_TurningPoints.RemoveAt(0);
                        }
                    }

                    SwimBehaviour.TargetPoint = m_TurningPoints[0];
                    if (Vector2.Distance(Fish.Position, SwimBehaviour.TargetPoint) <= 1f)
                    {
                        Debug.Log("到达拐点 " + m_TurningPoints[0]);
                        m_TurningPoints.RemoveAt(0);
                    }
                }
                else
                {
                    FishAI.PendingEndAbility(this, EEndAbilityType.End);
                    return;
                }
            }

            {
                Vector2 point = Fish.Position;
                foreach (var nextPoint in m_TurningPoints)
                {
                    Debug.DrawLine(point, nextPoint, Color.blue);
                    point = nextPoint;
                }

                Debug.DrawLine(point, m_PreTracingPoint, Color.blue);
            }
        }

        private void UpdateTurningPoint()
        {
            Vector2 startPoint;
            if (m_TurningPoints.Count == 0)
            {
                startPoint = Fish.Position;
            }
            else
            {
                startPoint = m_TurningPoints[^1];
            }

            Vector2 newPosition = TargetFish.Position;
            bool failed = false;
            //bool loseTarget = false;
            if (Vector2.Distance(startPoint, newPosition) >= m_TracingRayLength)
            {
                failed = true;    
            }
            else
            {
                if (Raycast(startPoint, newPosition))
                {
                    failed = true;
                }
            }

            if (failed)
            {
                if (m_HasPreTracingPoint)
                {
                    if (m_TurningPoints.Count < m_MaxTurningPointCount)
                    {
                        m_TurningPoints.Add(m_PreTracingPoint);
                        //Debug.Log("增加拐点 " + m_TurningPoints[^1]);
                        m_HasPreTracingPoint = true;
                        m_PreTracingPoint = newPosition;
                    }
                }
                else
                {
                    Debug.Log("因为找不到上个追踪点导致失去目标");
                    //loseTarget = true;
                }
            }
            else
            {
                m_HasPreTracingPoint = true;
                m_PreTracingPoint = newPosition;
            }
        }

    }
}