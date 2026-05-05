using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class FA_RaycastRangeTrace : FA_Trace
    {
        [Header("检测")]
        [SerializeField] private float m_SearchingRayLength = 7f;
        [SerializeField] private float m_TracingRayLength = 12f;
        [SerializeField] private int m_MaxTurningPointCount = 2;

        [Header("追逐保持")]
        [SerializeField] private float m_Patience = 100f;
        [SerializeField] private float m_LosePatienceSpeed = 30f;
        [SerializeField] private float m_RegainPatienceSpeed = 20f;
        [SerializeField] private float m_LosePatienceOnCatchFailed = 30f;
        [SerializeField] private float m_ChangeTargetPatienceTheshold = 50f;
        [SerializeField] private float m_SearchNewTargetIntervalTime = 0.2f;

        // Runtime
        [System.NonSerialized] private List<Vector2> m_TurningPoints = new();
        private bool m_HasPreTracingPoint;
        private Vector2 m_PreTracingPoint;

        [SerializeField] private float m_CurrentPatience;
        private float m_SearchNewTargetTimer;

        private RaycastHit2D Raycast(Vector2 start, Vector2 end)
            => FishUtils.RaycastObstacle(start, end);

        protected override bool FindTarget(out Fish outTarget)
            => FindTarget(out outTarget, false);
        protected bool FindTarget(out Fish outTarget, bool ignoreCurrentTarget)
        {
            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(Fish.Position, m_SearchingRayLength, fishList, ignoreFish: Fish, clearResultList: true);

            outTarget = null;
            int currentPriority = 0;

            foreach (var fish in fishList)
            {
                if ((!ignoreCurrentTarget || (ignoreCurrentTarget && fish != TargetFish))
                && TargetPriorityMap.ContainsKey(fish.FishTypeTag)
                && !Raycast(Fish.Position, fish.Position))
                {
                    if (outTarget == null)
                    {
                        outTarget = fish;
                        currentPriority = TargetPriorityMap[fish.FishTypeTag];
                    }
                    else
                    {
                        int p = TargetPriorityMap[fish.FishTypeTag];
                        if (p > currentPriority)
                        {
                            outTarget = fish;
                            currentPriority = p;
                        }
                        else if (p == currentPriority && fish.IsPlayer)
                        {
                            outTarget = fish;
                        }
                    }
                }
            }

            ListPool<Fish>.Release(fishList);
            return outTarget != null;
        }

        private Fish FindMorePriorityTarget()
        {
            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(Fish.Position, m_SearchingRayLength, fishList, ignoreFish: Fish, clearResultList: true);

            Fish newTarget = null;
            int currentPriority = TargetPriorityMap[TargetFish.FishTypeTag];

            foreach (var fish in fishList)
            {
                if (fish == TargetFish
                || !TargetPriorityMap.ContainsKey(fish.FishTypeTag)
                || Raycast(Fish.Position, fish.Position))
                    continue;
                
                int p = TargetPriorityMap[fish.FishTypeTag];
                if (p > currentPriority)
                {
                    newTarget = fish;
                    currentPriority = p;
                }
            }

            ListPool<Fish>.Release(fishList);
            return newTarget;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            m_TurningPoints.Clear();

            m_HasPreTracingPoint = true;
            m_PreTracingPoint = TargetFish.Position;

            m_CurrentPatience = m_Patience;

            if (Fish.TryGetBehaviour<FB_Eat>(out var eatBehaviour))
            {
                eatBehaviour.OnCatchFailed += OnCatchFailed;
            }
        }

        public override void OnEnd(EEndAbilityType endType)
        {
            base.OnEnd(endType);

            if (Fish != null && Fish.TryGetBehaviour<FB_Eat>(out var eatBehaviour))
            {
                eatBehaviour.OnCatchFailed -= OnCatchFailed;
            }
        }

        private void OnCatchFailed()
        {
            if (TargetFish != null)
            {
                Debug.Log("Catch Failed");
                m_CurrentPatience = Mathf.Max(1f, m_CurrentPatience - m_LosePatienceOnCatchFailed);
            }
        }

        private void SetTarget(Fish fish)
        {
            m_TurningPoints.Clear();

            TargetFish = fish;

            m_HasPreTracingPoint = true;
            m_PreTracingPoint = TargetFish.Position;
            m_CurrentPatience = m_Patience;

            if (Fish.TryGetBehaviour<FB_Eat>(out var eatBehaviour))
            {
                eatBehaviour.Target = fish;
            }
        }

        public override void OnUpdate(float dt)
        {
            if (TargetFish == null || !TargetFish.IsLiving)
            {
                TargetFish = null;
                FishAI.PendingEndAbility(this, EEndAbilityType.End);
                return;
            }

            if (m_CurrentPatience <= m_ChangeTargetPatienceTheshold)
            {
                m_SearchNewTargetTimer += dt;
                if (m_SearchNewTargetTimer >= m_SearchNewTargetIntervalTime)
                {
                    Debug.Log("Find New Target");
                    m_SearchNewTargetTimer = 0f;
                    if (FindTarget(out var newTarget, ignoreCurrentTarget: true))
                    {
                        SetTarget(newTarget);
                        return;
                    }
                }
            }

            // 如果可以直接追踪到目标，放弃所有拐点
            if (Vector2.Distance(Fish.Position, TargetFish.Position) <= m_TracingRayLength
            && !Raycast(Fish.Position, TargetFish.Position))
            {
                m_TurningPoints.Clear();
                m_PreTracingPoint = TargetFish.Position;
                m_HasPreTracingPoint = true;

                SwimBehaviour.TargetPoint = TargetFish.Position;

                m_CurrentPatience = Mathf.Clamp(m_CurrentPatience + dt * m_RegainPatienceSpeed, 0f, m_CurrentPatience);
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
                        if (!Raycast(Fish.Position, nextPoint))
                        {
                            m_TurningPoints.RemoveAt(0);
                        }
                    }

                    SwimBehaviour.TargetPoint = m_TurningPoints[0];
                    if (Vector2.Distance(Fish.Position, SwimBehaviour.TargetPoint) <= 1f)
                    {
                        // Debug.Log("到达拐点 " + m_TurningPoints[0]);
                        m_TurningPoints.RemoveAt(0);
                    }
                }
                else
                {
                    FishAI.PendingEndAbility(this, EEndAbilityType.End);
                    return;
                }

                // 失去耐心
                m_CurrentPatience -= dt * m_LosePatienceSpeed;
                if (m_CurrentPatience <= 0f)
                {
                    TargetFish = null;
                    FishAI.PendingEndAbility(this, EEndAbilityType.End);
                    return;
                }
            }

            // Debug
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