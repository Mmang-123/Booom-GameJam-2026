using Mmang.Game;
using Mmang.InputExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mmang.Topdown
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class TopdownCharacterController : MonoBehaviour
    {
        public CharacterController CharacterController { get; private set; }

        // 属性
        [Header("属性")]
        [SerializeField] private float m_MaxFallSpeed = 8f;
        
        // 组件
        [Header("组件")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private RootMotionTransmitter m_RootMotion;


        // Runtime
        public float FallSpeed { get; private set; }
        public Vector3 TargetDirection { get; private set; }
        public bool IsMoving { get; private set; }
        public bool IsSprinting { get; private set; }

        public bool Controllable { get; set; } = true;
        public bool Movable { get; set; } = true;


        // temp
        private InputAction m_MoveInputAction;

        private void Awake()
        {
            CharacterController = GetComponent<CharacterController>();

            TargetDirection = transform.forward;

            if (m_RootMotion != null)
            {
                m_RootMotion.OnMove += RootMotionUpdate;
            }

            m_MoveInputAction = InputSystemUtils.CreateTopdownVector2DInputAction();
            m_MoveInputAction.Enable();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            RotationUpdate(dt);
            ControlUpdate(dt);
        }


        private void RotationUpdate(float deltaTime)
        {
            Quaternion targetRotation = Quaternion.LookRotation(TargetDirection);
            //Debug.Log(targetRotation * Vector3.forward + "  " + TargetDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, deltaTime * 8f);
            //transform.rotation = targetRotation;
        }

        private void ControlUpdate(float deltaTime)
        {
            Vector3 direction = TopdownUtils.GetTopDownMoveDirection(m_MoveInputAction.ReadValue<Vector2>(), Camera.main.transform.rotation);
            if (!Controllable || !Movable)
                direction = Vector3.zero;

            IsMoving = direction != Vector3.zero;
            //IsSprinting = IsMoving && GameControlManager.SprintingPressed;
            IsSprinting = false;

            if (direction != Vector3.zero)
            {
                TargetDirection = direction;
            }

            if (m_Animator != null)
            {
                m_Animator.SetBool("IsMoving", IsMoving);
            }
        }

        private void RootMotionUpdate(Animator animator)
        {
            if (!CharacterController.isGrounded)
            {
                FallSpeed += Time.deltaTime * 10f;
                FallSpeed = Mathf.Clamp(FallSpeed, 0f, m_MaxFallSpeed);
            }
            else
            {
                FallSpeed = 0f;
            }

            Vector3 motion = new Vector3(animator.velocity.x, -FallSpeed, animator.velocity.z) * Time.deltaTime;
            CharacterController.Move(motion);
        }
    }

}