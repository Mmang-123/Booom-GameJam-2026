
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DynamicUI : MonoBehaviour
    {
        private SpriteRenderer m_Renderer;
        [SerializeField] private Sprite m_MouseSprite;
        [SerializeField] private Sprite m_KeyboardSprite;
        [SerializeField] private Sprite m_GamepadSprite;

        private void Start()
        {
            m_Renderer = GetComponent<SpriteRenderer>();
            OnDeviceChanged(GameInputManager.CurrentControlScheme);
        }
        
        private void OnEnable()
        {
            GameInputManager.OnInputDeviceChanged += OnDeviceChanged;
        }

        private void OnDisable()
        {
            if (GameInputManager.InstanceValid)
                GameInputManager.OnInputDeviceChanged -= OnDeviceChanged;
        }

        public void OnDeviceChanged(GameInputManager.EControlMode controlMode)
        {
            switch (controlMode)
            {
                case GameInputManager.EControlMode.Mouse:
                    m_Renderer.sprite = m_MouseSprite;
                    break;
                case GameInputManager.EControlMode.Keyboard:
                    m_Renderer.sprite = m_KeyboardSprite;
                    break;
                case GameInputManager.EControlMode.Gamepad:
                    m_Renderer.sprite = m_GamepadSprite;
                    break;
            }
        }
    }
}