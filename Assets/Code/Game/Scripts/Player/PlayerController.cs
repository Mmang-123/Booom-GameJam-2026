using System.Collections.Generic;
using Mmang.Game;
using Mmang.Util;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace Game
{
    public class PlayerController : SingletonMono<PlayerController>, IFishController
    {
        [SerializeField] private Fish m_CurrentFish;
        [SerializeField] private float m_LookAtOffset = 0.5f;
        [SerializeField] private float m_LookAtOffsetMouseRadius = 2f; // 根据鼠标到中心的距离/半径 作为偏移的强度

        public Fish Fish { get; private set; }

        public bool Active { get; private set; } = true;
        public float DisableTimer { get; set; }

        // Runtime
        private Transform m_PlayerDirectionPoint; // 根据朝向实时更新的点
        private bool m_MouseRBPressedLastFrame = false;
        private ControlFishConfig m_FishConfig;

        protected override void OnAwake()
        {
            GameObject pointGO = new("Player Direction Point");
            m_PlayerDirectionPoint = pointGO.transform;
            
            ControlFish(m_CurrentFish);
        }

        public void ControlFish(Fish fish)
        {
            fish.SetController(this);
            SetFish(fish);

            if (fish.TryGetBehaviour<FB_Swim>(out var behaviour))
            {
                behaviour.CanAvoidance = false;
            }

            m_FishConfig = PlayerConfig.GetConfig(fish.FishTypeTag);
        }

        public void LoseControl(IFishController newController)
        {
            // 这大概是不会发生的
            Debug.Log("发生了什么???");
            SetFish(null);
        }

        private void SetFish(Fish fish)
        {
            Fish = fish;
            var cameraController = CameraController.Instance;
            if (m_CurrentFish != null)
            {
                cameraController.SetMainTarget(null);
            } 
            
            m_CurrentFish = fish;

            if (fish == null)
            {
                cameraController.RemoveFollowPoint(m_PlayerDirectionPoint);
            }
            else
            {
                m_CurrentFish.Init();
                m_CurrentFish.GetBehaviour<FB_Swim>().RotateToTargetPoint = Active;
                cameraController.AddFollowPoint(m_PlayerDirectionPoint, m_LookAtOffset);
                cameraController.SetMainTarget(fish.transform);

                // 水母感染等级直接设置为2
                if (m_CurrentFish.FishTypeTag.Equals(GameplayTag.CreateByName("FishType.JellyGleam")))
                {
                    m_CurrentFish.SetInfectedLevel(EInfectedLevel.High);
                }
            }
        }

        private void Update()
        {
            if (!Active)
            {
                if (DisableTimer > 0f)
                {
                    DisableTimer -= Time.deltaTime;
                    if (DisableTimer <= 0f)
                    {
                        SetControlActive(true);
                    }
                }
                return;
            }

            TraceMousePoint();

            if (Mouse.current.rightButton.isPressed && !m_MouseRBPressedLastFrame)
            {
                UseSkill();
            }
        }

        private void FixedUpdate()
        {
            if (m_FishConfig != null && m_FishConfig.CanEatTags.Count > 0)
            {
                HuntUpdate();
            }
        }

        private void LateUpdate()
        {
            m_MouseRBPressedLastFrame = Mouse.current.rightButton.isPressed;
        }

        public void SetControlActive(bool active)
        {
            if (Active == active)
                return;
            
            Active = active;
            if (m_CurrentFish != null)
            {
                m_CurrentFish.GetBehaviour<FB_Swim>().RotateToTargetPoint = Active;
            }
        }

        public void DisableControl(float disableTime)
        {
            SetControlActive(false);
            DisableTimer = disableTime;
        }

        private void TraceMousePoint()
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            if (m_CurrentFish != null)
            {
                var swimBehaviour = m_CurrentFish.GetBehaviour<FB_Swim>();
                swimBehaviour.TargetPoint = worldPos;
                swimBehaviour.Tracing = Mouse.current.leftButton.isPressed;

                // 相机偏移
                float distance = Vector2.Distance(worldPos, m_CurrentFish.Position);
                float t = Mathf.Min(1f, distance / m_LookAtOffsetMouseRadius);

                m_PlayerDirectionPoint.transform.position = m_CurrentFish.Position
                    + m_CurrentFish.ForwardDirection * t;


                // 如果是水母, Idle时自动回正
                if (m_CurrentFish.FishTypeTag.Equals(GameplayTag.CreateByName("FishType.JellyGleam")))
                {
                    if (!swimBehaviour.Tracing && swimBehaviour.CurrentSpeed <= 0.1f)
                    {
                        swimBehaviour.TargetPoint = m_CurrentFish.Position + Vector2.up;
                    }
                }
            }
        }

        private void UseSkill()
        {
            if (m_CurrentFish != null
            && m_CurrentFish.TryGetBehaviour<FB_Skill>(out var skillBehaviour))
            {
                if (skillBehaviour.CanUse())
                {
                    skillBehaviour.Use();
                }    
            }
        }


        #region 捕食

        private void HuntUpdate()
        {
            var target = FindTargetInRange(out float distance);
            var eatBehaviour = Fish.GetBehaviour<FB_Eat>();
            eatBehaviour.Target = target;
            eatBehaviour.ContinuousCheck = true;
        }

        private Fish FindTargetInRange(out float nearestDistance)
        {
            List<Fish> fishList = ListPool<Fish>.Get();
            FishUtils.GetFishInCircle(Fish.Position, m_FishConfig.OpenMouseDistance, fishList, ignoreFish: Fish, clearResultList: true);

            Fish result = null;
            nearestDistance = float.MaxValue;

            foreach (var fish in fishList)
            {
                if (!m_FishConfig.CanEatTags.Contains(fish.FishTypeTag))
                    continue;

                // 距离
                float distance = Vector2.Distance(fish.Position, Fish.Position);
                if (distance > nearestDistance)
                    continue;

                // 角度
                float angle = Vector2.Angle(Fish.ForwardDirection, (fish.Position - Fish.Position).normalized);
                if (angle > m_FishConfig.OpenMouseAngle)
                    continue;

                // 障碍检测
                var hit = FishUtils.RaycastObstacle(Fish.Position, fish.Position);
                if (!hit)
                {
                    result = fish;
                    nearestDistance = distance;
                }
            }

            ListPool<Fish>.Release(fishList);
            return result;
        }


        #endregion
    }
}