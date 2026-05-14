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

        [SerializeField] private float m_SuicideTime = 2f;
        [SerializeField] private CircularProgressBar m_CircularProgressBar;
        [SerializeField] private CircleRange m_CircleRange;

        public Fish Fish => m_CurrentFish;

        public bool Active { get; private set; } = true;
        public float DisableTimer { get; set; }

        private float MinMBPressedTime => 0.25f;
        private float MinLBPressedTime => 0.4f;

        // Runtime
        private Transform m_PlayerDirectionPoint; // 根据朝向实时更新的点
        private bool m_MouseRBPressedLastFrame = false;
        private bool m_MBPressed = false;
        private float m_MBPressedTimer = 0f;

        private Vector2 m_LastLBPosition;
        private float m_LBTimer;
        private bool m_LBPressed;


        private ControlFishConfig m_FishConfig;
        public ControlFishConfig FishConfig => m_FishConfig;

        private float m_RestartTimer = 0f;

        protected override void OnAwake()
        {
            GameObject pointGO = new("Player Direction Point");
            m_PlayerDirectionPoint = pointGO.transform;
            
            ControlFish(m_CurrentFish, true);
        }

        public void ControlFish(Fish fish) => ControlFish(fish, false);
        public void ControlFish(Fish fish, bool force)
        {
            if ((!force && fish == m_CurrentFish) || fish == null)
                return;

            m_FishConfig = PlayerConfig.GetConfig(fish.FishTypeTag);

            fish.SetController(this);
            SetFish(fish);

            if (fish.TryGetBehaviour<FB_Swim>(out var behaviour))
            {
                behaviour.CanAvoidance = false;
            }

            if (fish.TryGetBehaviour<FB_Eat>(out var eatBehaviour))
            {
                eatBehaviour.UseOverrideEatDistance = true;
                eatBehaviour.OverrideEatDistance = m_FishConfig.EatDistance;
            }
        }

        public void LoseControl(Fish fish)
        {
            if (fish == Fish)
                SetFish(null);
            m_RestartTimer = 0f;
        }

        private void SetFish(Fish fish)
        {
            Debug.Log("Control: " + fish);
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
                // 放到场景根下
                m_CurrentFish.transform.SetParent(null, true);

                //
                if (!m_CurrentFish.Equals(FishUtils.GolemFishTag))
                {
                    DarkSightManager.Instance.SetRadius(0f);
                }

                //
                var healthBar = CameraController.Instance.HealthBar;
                if (Fish.FishTypeTag.Equals(FishUtils.JellyGleamTag))
                {
                    healthBar.gameObject.SetActive(false);
                }
                else
                {
                    healthBar.SetColor(m_CurrentFish.BodyColor);
                    healthBar.SetSegmentCount(Mathf.RoundToInt(m_CurrentFish.MaxSaturation / 60));
                    healthBar.gameObject.SetActive(true);
                }
                //
                m_CurrentFish.Init();
                m_CurrentFish.GetBehaviour<FB_Swim>().RotateToTargetPoint = Active;
                m_PlayerDirectionPoint.position = m_CurrentFish.Position;
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
            if ((Fish == null)
            && GameManager.Instance.CanRestart)
            {
                m_RestartTimer += Time.deltaTime;
                if (m_RestartTimer > 1.0f)
                {
                    GameManager.Instance.Restart(LevelConfig.GetInitLevelName());
                    m_RestartTimer = 0f;
                }
            }
            else
            {
                m_RestartTimer = 0f;
            }

            if (Fish == null)
            {
                return;
            }

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

            var mouse = Mouse.current;
            if (mouse != null)
            {
                TraceMousePoint();

                if (mouse.rightButton.isPressed && !m_MouseRBPressedLastFrame)
                {
                    UseSkill();
                }

                MBUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (Fish == null)
            {
                return;
            }
            
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
            var mouse = Mouse.current;
            Vector2 screenPos = mouse.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            if (m_CurrentFish != null)
            {
                bool leftPressed = mouse.leftButton.isPressed;
                var swimBehaviour = m_CurrentFish.GetBehaviour<FB_Swim>();

                if (!m_LBPressed && leftPressed)
                {
                    m_LBPressed = true;
                    m_LBTimer = 0f;
                }
                else
                {
                    m_LBTimer += Time.deltaTime;
                    if (!leftPressed && m_LBTimer > MinLBPressedTime)
                    {
                        m_LBPressed = false;
                    }
                }


                if (!m_CurrentFish.FishTypeTag.Equals(GameplayTag.CreateByName("FishType.JellyGleam"))
                || m_LBPressed || swimBehaviour.CurrentSpeed > 0.1f)
                {
                    Vector2 targetPoint = (!m_LBPressed || leftPressed) ? worldPos : m_LastLBPosition;
                    m_LastLBPosition = targetPoint;
                    swimBehaviour.TargetPoint = targetPoint;
                }
                swimBehaviour.Tracing = m_LBPressed;

                // 相机偏移
                float distance = Vector2.Distance(worldPos, m_CurrentFish.Position);
                float t = Mathf.Min(1f, distance / m_LookAtOffsetMouseRadius);

                m_PlayerDirectionPoint.transform.position = m_CurrentFish.Position
                    + ((Vector2)worldPos - m_CurrentFish.Position).normalized * t;


            }
        }

        private void MBUpdate()
        {
            var mouse = Mouse.current;

            if (Fish == null || !Fish.IsLiving || !GameManager.Instance.CanRestart)
            {
                if (m_MBPressed)
                {
                    //m_CircleRange.FadeOut();
                    m_MBPressed = false;
                    m_MBPressedTimer = 0f;
                    UpdateSuicide();
                }

                return;
            }

            if (!m_MBPressed && m_MBPressedTimer <= 0f && mouse.middleButton.isPressed)
            {
                InitSuicide();
                //m_CircleRange.FadeIn(Fish, m_FishConfig);
            }
            else
            {
                if (m_MBPressed)
                    m_MBPressedTimer += Time.deltaTime;
                else
                    m_MBPressedTimer -= Time.deltaTime;
                
                if (m_MBPressedTimer > MinMBPressedTime && !mouse.middleButton.isPressed)
                {
                    m_MBPressed = false;
                    //m_CircleRange.FadeOut();
                }
                else if (mouse.middleButton.isPressed)
                {
                    m_MBPressed = true;
                    //m_CircleRange.FadeIn(Fish, m_FishConfig);
                }

                UpdateSuicide();
                if (!m_MBPressed && m_MBPressedTimer <= 0f)
                {
                    m_CircularProgressBar.AnimateOut();
                }
                else if (m_MBPressedTimer > m_SuicideTime)
                {
                    CommitSuicide();
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

        private void InitSuicide()
        {
            m_CircularProgressBar.transform.position = m_CurrentFish.Position;
            m_CircularProgressBar.SetT(0f);
            m_CircularProgressBar.SetFish(m_CurrentFish, m_FishConfig);
            m_CircularProgressBar.AnimateIn();
            m_MBPressed = true;
            m_MBPressedTimer = 0f;
        }

        private void UpdateSuicide()
        {
            m_CircularProgressBar.transform.position = m_CurrentFish.Position;
            m_CircleRange.transform.position = m_CurrentFish.Position;
            float t = Mathf.Clamp01(m_MBPressedTimer / m_SuicideTime);
            m_CircularProgressBar.SetT(t);
        }

        private void CommitSuicide()
        {
            m_MBPressedTimer = 0f;
            m_MBPressed = false;
            m_CircularProgressBar.AnimateOut();
            if (Fish == null)
                return;

            Fish.Die(EDieType.Hunger);
            MiddleButtonTips.Triggered = true;
        }

        public void Transfer(Vector2 newPosition)
        {
            if (Fish == null)
                return;
            Vector2 offset = newPosition - Fish.Position;
            Fish.SetPosition(newPosition);
            m_PlayerDirectionPoint.transform.position += (Vector3)offset;
            CameraController.Instance.TransferOffset(offset);
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