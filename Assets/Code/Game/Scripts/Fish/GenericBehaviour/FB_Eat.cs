using System.Collections.Generic;
using Mmang.Game;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class FB_Eat : FishBehaviour
    {
        public enum EState
        {
            Shut, Open, Eat, Wait
        }
        
        [SerializeField] private float m_OpenDistance = 3f;
        [SerializeField] private float m_ShutDistance = 4f;

        [SerializeField] private float m_EatDistance = 1.5f;
        [SerializeField] private float m_ReduceSaturationPerBite = 50f;
        [SerializeField] private float m_EatDashAdditionalSpeed = 3f;
        [SerializeField] private float m_EatDashAdditionalRotateSpeed = 3f;
        [SerializeField] private List<CircleCollider2D> m_EatRanges;
        [SerializeField] private float m_WaitTimeAfterEat = 1f;

        [SerializeField] private GameplayTagContainer m_CanEatTags = new();

        // Runtime
        public Fish Target { get; set; }
        public EState State { get; private set; }
        public float EatTimer { get; private set; }
        public float WaitTimer { get; private set; }

        private void Update()
        {
            switch (State)
            {
                case EState.Shut:
                    ShutUpdate();
                    break;
                case EState.Open:
                    OpenUpdate();
                    break;
                case EState.Eat:
                    EatUpdate();
                    break;
                case EState.Wait:
                    WaitUpdate();
                    break;
            }
        }

        private void ShutUpdate()
        {
            if (Target != null)
            {
                float distance = Vector2.Distance(Fish.Position, Target.Position);
                if (distance <= m_OpenDistance)
                {
                    State = EState.Open;
                    return;
                }
            }
        }

        private void OpenUpdate()
        {
            if (Target != null)
            {
                float distance = Vector2.Distance(Fish.Position, Target.Position);
                if (distance > m_ShutDistance)
                {
                    State = EState.Shut;
                    return;
                }
                if (distance <= m_EatDistance)
                {
                    PreEat();
                    return;
                }
            }
            else
            {
                State = EState.Shut;
            }
        }

        private void EatUpdate()
        {
            if (Target != null)
            {
                EatTimer += Time.deltaTime;   
                if (EatTimer >= 0.25f)
                {
                    Eat();
                    return;
                }
            }
            else
            {
                EatEnd();
                return;
            }
        }

        private void WaitUpdate()
        {
            if (WaitTimer > 0f)
            {
                WaitTimer -= Time.deltaTime;
            }
            else
            {
                State = EState.Shut;
            }
        }

        private void PreEat()
        {
            State = EState.Eat;
            EatTimer = 0f;

            // 动画
            if (Fish.TryGetBehaviour<FB_GenericAnimator>(out var animatorBehaviour))
            {
                animatorBehaviour.TriggerCatchAnimation();
            }

            var swimBehaviour = Fish.GetBehaviour<FB_Swim>();
            swimBehaviour.AdditionalSpeed += m_EatDashAdditionalSpeed;
            swimBehaviour.AdditionalRotateSpeed += m_EatDashAdditionalRotateSpeed;
        }

        private void Eat()
        {
            // 检测
            List<Fish> fishInRange = ListPool<Fish>.Get();
            List<Fish> toEat = ListPool<Fish>.Get();

            foreach (var range in m_EatRanges)
            {
                Vector2 center = (Vector2)range.transform.position + range.offset;
                float radius = range.radius;
                FishUtils.GetFishInCircle(center, radius, fishInRange, ignoreFish: Fish, clearResultList: false);
            }
            
            float reduceSaturation = m_ReduceSaturationPerBite;
            
            foreach (var fish in fishInRange)
            {
                if (!m_CanEatTags.Contains(fish.FishTypeTag))
                {
                    continue;
                }

                if (fish.Saturation > reduceSaturation)
                {
                    fish.RemoveSaturation(reduceSaturation);
                }
                else
                {
                    toEat.Add(fish);
                }
            }

            // 吞食
            bool infected = false;
            foreach (var fish in toEat)
            {
                if (fish.IsPlayer)
                {
                    infected = true;
                    Fish.AddInfectedLevel();
                    PlayerController.Instance.DisableControl(1.2f);
                    PlayerController.Instance.ControlFish(Fish);
                    Target = null;
                }
                fish.Die(EDieType.Eaten);
            }

            // 动画
            if (Fish.TryGetBehaviour<FB_GenericAnimator>(out var animatorBehaviour))
            {
                animatorBehaviour.TriggerSwallowAnimation(infected);
            }
            
            ListPool<Fish>.Release(fishInRange);
            ListPool<Fish>.Release(toEat);
            
            
            //
            EatEnd();
        }

        private void EatEnd()
        {
            State = EState.Wait;
            WaitTimer = m_WaitTimeAfterEat;

            var swimBehaviour = Fish.GetBehaviour<FB_Swim>();
            swimBehaviour.AdditionalSpeed -= m_EatDashAdditionalSpeed;
            swimBehaviour.AdditionalRotateSpeed -= m_EatDashAdditionalRotateSpeed;
        }

    }
}