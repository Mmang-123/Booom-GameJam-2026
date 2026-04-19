using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using Mmang.InputExtensions;

namespace Mmang.Topdown
{
    [RequireComponent(typeof(TopdownCameraController))]
    public class FlyCamera : MonoBehaviour
    {
        [SerializeField] private float m_MoveSpeed = 5f;
        [SerializeField] private float m_RotateTime = 0.25f;
        public TopdownCameraController CameraController { get; private set; }

        public Vector3 TargetRotationEuler { get; private set; }

        private InputAction m_MoveInputAction;

        private void Awake()
        {
            TargetRotationEuler = transform.rotation.eulerAngles;
            CameraController = GetComponent<TopdownCameraController>();

            m_MoveInputAction = InputSystemUtils.CreateTopdownVector2DInputAction();
            m_MoveInputAction.Enable();
        }

        private void Update()
        {
            if (m_MoveInputAction != null)
            {
                Vector3 direction = TopdownUtils.GetTopDownMoveDirection(m_MoveInputAction.ReadValue<Vector2>(), transform.rotation);
                CameraController.CenterPosition += m_MoveSpeed * Time.deltaTime * direction;
            }
            
            if (Input.GetKeyDown(KeyCode.Q))
                TurnLeft();
            if (Input.GetKeyDown(KeyCode.E))
                TurnRight();
            
        }

        private void FixedUpdate()
        {
            transform.rotation = Quaternion.Euler(TargetRotationEuler);
        }

        private void TurnLeft()
        {
            TargetRotationEuler += new Vector3(0f, 45f, 0f);
            transform.DORotate(TargetRotationEuler, m_RotateTime);
        }

        private void TurnRight()
        {
            TargetRotationEuler -= new Vector3(0f, 45f, 0f);
            transform.DORotate(TargetRotationEuler, m_RotateTime);
        }
    }
}