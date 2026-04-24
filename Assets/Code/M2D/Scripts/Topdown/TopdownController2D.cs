using Mmang.InputExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mmang.M2D
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(KinematicBody2D))]
    public class TopdownController2D : MonoBehaviour
    {
        [SerializeField] private float m_Speed;

        // Runtime
        private KinematicBody2D m_Body;
        private InputAction m_MoveInputAction;

        private void Awake()
        {
            m_Body = GetComponent<KinematicBody2D>();

            m_MoveInputAction = InputSystemUtils.CreateTopdownVector2DInputAction();
            m_MoveInputAction.Enable();
        }

        private void Update()
        {
            MoveUpdate(Time.deltaTime);
        }

        private void MoveUpdate(float dt)
        {
            Vector2 direction = m_MoveInputAction.ReadValue<Vector2>();
            Vector2 motion = m_Speed * dt * direction;

            m_Body.Move(motion);
        }
        
    }

}