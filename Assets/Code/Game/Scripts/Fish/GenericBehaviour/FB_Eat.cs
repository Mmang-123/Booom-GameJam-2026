using System.Collections.Generic;
using DG.Tweening;
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
        [SerializeField] private float m_EatAnimationDuration = 0.125f;

        [Header("吞食范围前移")]
        [SerializeField] private float m_MaxEatRangeOffset = 0.8f;
        [SerializeField] private float m_MaxEatRangeOffsetRequireSpeed = 4f;

        [SerializeField] private GameplayTagContainer m_CanEatTags = new();

        // Runtime
        public Fish Target { get; set; }
        public EState State { get; private set; }
        public float EatTimer { get; private set; }
        public float WaitTimer { get; private set; }
        public bool ContinuousCheck { get; set; } = false;

        public bool UseOverrideEatDistance { get; set; }
        public float OverrideEatDistance { get; set; }

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
                float eatDistance = UseOverrideEatDistance ? OverrideEatDistance : m_EatDistance;
                if (distance > m_ShutDistance)
                {
                    State = EState.Shut;
                    return;
                }
                if (distance <= eatDistance)
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
                if ((ContinuousCheck && EatTimer >= 0.08f)
                || EatTimer >= 0.12f)
                {
                    List<Fish> fishInRange = ListPool<Fish>.Get();
                    CheckFishInRange(fishInRange);

                    if (fishInRange.Count > 0)
                    {
                        ApplyEat(fishInRange);
                        ListPool<Fish>.Release(fishInRange);
                        return;
                    }

                    ListPool<Fish>.Release(fishInRange);
                }
  
                if (EatTimer >= 0.28f)
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
            CheckFishInRange(fishInRange);

            ApplyEat(fishInRange);
            ListPool<Fish>.Release(fishInRange);
        }

        private void CheckFishInRange(List<Fish> fishInRange)
        {
            bool moveForward = true; // 只对第一个碰撞体有效
            foreach (var range in m_EatRanges)
            {
                Vector2 center = (Vector2)range.transform.position + range.offset;
                float radius = range.radius;

                if (moveForward)
                {
                    var swimBehaviour = Fish.GetBehaviour<FB_Swim>();
                    float offset = m_MaxEatRangeOffset * Mathf.Clamp01(swimBehaviour.CurrentSpeed / m_MaxEatRangeOffsetRequireSpeed);
                    center += Fish.ForwardDirection * offset;
                }

                FishUtils.GetFishInCircle(center, radius, fishInRange, ignoreFish: Fish, clearResultList: false);
                moveForward = false;
            }
        }

        private void ApplyEat(List<Fish> fishInRange)
        {
            List<Fish> toEat = ListPool<Fish>.Get();
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
            CircleCollider2D suckTarget = m_EatRanges.Count > 0 ? m_EatRanges[0] : null;
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

                if (suckTarget != null)
                    StartSuckAnimation(fish, suckTarget);
                else
                    fish.Die(EDieType.Eaten);
            }

            // 动画
            if (Fish.TryGetBehaviour<FB_GenericAnimator>(out var animatorBehaviour))
            {
                animatorBehaviour.TriggerSwallowAnimation(infected);
            }
            
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

        private void StartSuckAnimation(Fish fish, CircleCollider2D collider)
        {
            // 停止被吃鱼的一切行为
            foreach (var behaviour in fish.GetComponents<FishBehaviour>())
                behaviour.enabled = false;

            var rb = fish.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.bodyType = RigidbodyType2D.Kinematic;

            Vector3 startPos = fish.transform.position;
            float progress = 0f;
            var predator = Fish; // 捕获捕食者引用，避免闭包访问 this

            Sequence seq = null;
            seq = DOTween.Sequence();

            // 跟随捕食者的吃范围中心（实时更新目标位置）
            seq.Join(
                DOTween.To(
                    () => progress,
                    p =>
                    {
                        // 捕食者已死，提前结束
                        if (predator == null || !predator.gameObject.activeSelf)
                        {
                            seq?.Kill();
                            fish.transform.localScale = Vector3.one;
                            fish.Die(EDieType.Eaten);
                            return;
                        }

                        progress = p;
                        if (collider == null) return;
                        Vector3 target = collider.transform.position + (Vector3)collider.offset;
                        fish.transform.position = Vector3.LerpUnclamped(startPos, target, p);
                    },
                    1f,
                    m_EatAnimationDuration
                ).SetEase(Ease.OutSine)
            );

            // 缩小到零
            seq.Join(
                fish.transform.DOScale(Vector3.zero, m_EatAnimationDuration).SetEase(Ease.OutSine)
            );

            seq.OnComplete(() =>
            {
                fish.transform.localScale = Vector3.one;
                fish.Die(EDieType.Eaten);
            });
        }

    }
}