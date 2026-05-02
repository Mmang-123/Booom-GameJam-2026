using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Fish m_CurrentFish;
        [SerializeField] private float m_LookAtOffset = 0.5f;
        [SerializeField] private float m_LookAtOffsetMouseRadius = 2f; // 根据鼠标到中心的距离/半径 作为偏移的强度

        // Runtime
        private Transform m_PlayerDirectionPoint; // 根据朝向实时更新的点
        private bool m_MouseRBPressedLastFrame = false;

        private void Awake()
        {
            GameObject pointGO = new("Player Direction Point");
            m_PlayerDirectionPoint = pointGO.transform;
            
            SetFish(m_CurrentFish);
        }

        private void SetFish(Fish fish)
        {
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
                m_CurrentFish.GetBehaviour<FB_Swim>().RotateToTargetPoint = true;
                cameraController.AddFollowPoint(m_PlayerDirectionPoint, m_LookAtOffset);
                cameraController.SetMainTarget(fish.transform);
            }
        }

        private void Update()
        {
            TraceMousePoint();

            if (Mouse.current.rightButton.isPressed && !m_MouseRBPressedLastFrame)
            {
                UseSkill();
            }
            m_MouseRBPressedLastFrame = Mouse.current.rightButton.isPressed;
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
    }
}