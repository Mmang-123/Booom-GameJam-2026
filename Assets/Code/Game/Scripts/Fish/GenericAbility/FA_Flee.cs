using System.Collections.Generic;
using Mmang.Game;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class FA_Flee : FishAIAbility
    {
        [SerializeField] private GameplayTagContainer m_FleeFromTags = new();
        [SerializeField] private float m_StartRadius = 8f;
        [SerializeField] private float m_EndRadius = 16f;

        [SerializeField] private Vector2 m_RandomRange = new(-30f, 30f);
        [SerializeField] private float m_ChangeDirectionDistance = 4f;
        [SerializeField] private Vector2 m_RandomChangeDirectionTime = new(1f, 4f);
        [SerializeField] private float m_ChangeDirectionCD = 1.5f;
        [SerializeField] private float m_LoseTargetTime = 2f;

        // Runtime
        private Fish FleeTarget { get; set; }
        private Vector2 FleeDirection { get; set; }

        private FB_Swim SwimBehaviour { get; set; }
        private float RandomChangeDirectionTime { get; set; }
        private float RandomChangeDirectionTimer { get; set; }
        private float ChangeDirectionCD { get; set; }
        private float LoseTargetTimer { get; set; }

        public override bool CanActivateAbility()
        {
            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(Fish.Position, m_StartRadius, fishList, ignoreFish: Fish, clearResultList: true);

            Fish target = null;

            foreach (var fish in fishList)
            {
                if (!m_FleeFromTags.Contains(fish.FishTypeTag)
                || FishUtils.RaycastObstacle(Fish.Position, fish.Position))
                    continue;

                target = fish;
                break;
            }

            FleeTarget = target;
            return target != null;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            Debug.Log("逃离 " + FleeTarget);
            SwimBehaviour = Fish.GetBehaviour<FB_Swim>();
            SwimBehaviour.Tracing = true;
            LoseTargetTimer = 0f;

            GetFleeDirection();
        }

        public override void OnEnd(EEndAbilityType endType)
        {
            base.OnEnd(endType);

        }

        public override void OnUpdate(float dt)
        {
            float distance = Vector2.Distance(Fish.Position, FleeTarget.Position);
            if (distance > m_EndRadius)
            {
                FishAI.PendingEndAbility(this, EEndAbilityType.End);
                return;
            }

            if (FishUtils.RaycastObstacle(Fish.Position, FleeTarget.Position))
            {
                LoseTargetTimer += dt;
                if (LoseTargetTimer > m_LoseTargetTime)
                {
                    FishAI.PendingEndAbility(this, EEndAbilityType.End);
                    return;
                }
            }
            else
            {
                LoseTargetTimer = 0f;
            }

            SwimBehaviour.TargetPoint = Fish.Position + FleeDirection;
            
            if (ChangeDirectionCD > 0f)
            {
                ChangeDirectionCD -= dt;
            }
            else
            {
                var hit = FishUtils.RaycastObstacle(Fish.Position, FleeDirection, m_ChangeDirectionDistance);
                if (hit)
                {
                    GetFleeDirection();
                }
                else
                {
                    RandomChangeDirectionTimer += dt;
                    if (RandomChangeDirectionTimer >= RandomChangeDirectionTime)
                    {
                        GetFleeDirection();
                    }
                }
            }
        }

        private void GetFleeDirection()
        {
            Vector2 baseDirection = (Fish.Position - FleeTarget.Position).normalized;
            float offsetAngle = Random.Range(m_RandomRange.x, m_RandomRange.y);
            float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

            var rotation = Quaternion.Euler(0f, 0f, baseAngle + offsetAngle + 180f);
            Vector2 direction = rotation * Vector2.left;
            Debug.DrawLine(Fish.Position, Fish.Position + direction, Color.azure, 10f);
            FleeDirection = direction;

            //
            RandomChangeDirectionTime = Random.Range(m_RandomChangeDirectionTime.x, m_RandomChangeDirectionTime.y);
            RandomChangeDirectionTimer = 0f;
            ChangeDirectionCD = m_ChangeDirectionCD;
        }
    }
}