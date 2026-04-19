using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace Mmang.InputExtensions
{
    [InputControlLayout(displayName = "平台跳跃角色方向输入")]
    public class PlatformerVector2DComposite : InputBindingComposite<Vector2>
    {
        [Tooltip("Up button input")]
        [InputControl(layout = "Button")]
        public int up;

        [Tooltip("Down button input")]
        [InputControl(layout = "Button")]
        public int down;

        [Tooltip("Left button input")]
        [InputControl(layout = "Button")]
        public int left;

        [Tooltip("Right button input")]
        [InputControl(layout = "Button")]
        public int right;

        bool m_UpPressed, m_DownPressed, m_LeftPressed, m_RightPressed;
        DirectionStates m_Directions;

        // 更强调快速响应的按键输入检测
        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            bool upPressed = context.ReadValueAsButton(up);
            bool downPressed = context.ReadValueAsButton(down);
            bool leftPressed = context.ReadValueAsButton(left);
            bool rightPressed = context.ReadValueAsButton(right);

            if (upPressed && !m_UpPressed)
            {
                m_Directions.VerticalState = EDirectionState.Positive;
            }
            else if (downPressed && !m_DownPressed)
            {
                m_Directions.VerticalState = EDirectionState.Negative;
            }
            else if (!upPressed && !downPressed)
            {
                m_Directions.VerticalState = EDirectionState.None;
            }

            if (rightPressed && !m_RightPressed)
            {
                m_Directions.HorizontalState = EDirectionState.Positive;
            }
            else if (leftPressed && !m_LeftPressed)
            {
                m_Directions.HorizontalState = EDirectionState.Negative;
            }
            else if (!rightPressed && !leftPressed)
            {
                m_Directions.HorizontalState = EDirectionState.None;
            }

            m_UpPressed = upPressed;
            m_DownPressed = downPressed;
            m_LeftPressed = leftPressed;
            m_RightPressed = rightPressed;

            return m_Directions.GetVector();
        }
    }

}