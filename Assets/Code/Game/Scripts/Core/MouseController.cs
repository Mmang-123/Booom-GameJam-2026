using Mmang.PixelartRender;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class MouseController : MonoBehaviour
    {
        [SerializeField] private Animator m_MouseAnimator;
        [SerializeField] private SpriteRenderer m_Renderer;

        private void OnEnable()
        {
            Cursor.visible = false;    
        }

        private void OnDisable()
        {
            Cursor.visible = true;
        }

        private void Update()
        {
            var cameraController = CameraController.Instance;

            if (GameInputManager.CurrentControlScheme == GameInputManager.EControlMode.Mouse)
            {
                m_Renderer.enabled = true;
                var mouse = Mouse.current;
                // 获取鼠标在屏幕上的位置 (像素坐标)
                Vector3 mouseScreenPosition = mouse.position.ReadValue();

                // 鼠标的 Z 轴通常需要与摄像机的距离匹配，在 2D 中通常设为摄像机的 nearClipPlane 即可
                mouseScreenPosition.z = Camera.main.nearClipPlane;

                // 将屏幕坐标转换为世界坐标
                Vector3 worldPosition = cameraController.PixelartCamera.Camera.ScreenToWorldPoint(mouseScreenPosition);

                // 为了防止 2D 游戏中光标的 Z 轴影响渲染，强制将 Z 轴设为 0
                worldPosition.z = 0f;

                // 更新当前精灵图（也就是你的自定义光标）的位置
                transform.position = worldPosition;

                m_MouseAnimator.SetBool("Pressed", mouse.leftButton.isPressed);
                m_MouseAnimator.SetBool("Enable", true);
            }
            else
            {
                var direction = GameInputManager.GetDirection();
                if (direction == Vector2.zero)
                {
                    m_MouseAnimator.SetBool("Pressed", false);
                    m_MouseAnimator.SetBool("Enable", false);
                    m_Renderer.enabled = false;
                    return;
                }
                m_Renderer.enabled = true;
                transform.position = GameInputManager.VirtualMousePosition;
                m_MouseAnimator.SetBool("Pressed", true);
                m_MouseAnimator.SetBool("Enable", true);
            }
        }
    }
}