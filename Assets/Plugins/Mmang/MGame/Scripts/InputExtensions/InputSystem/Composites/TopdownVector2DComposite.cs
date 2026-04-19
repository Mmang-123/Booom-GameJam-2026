using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace Mmang.InputExtensions
{
    [InputControlLayout(displayName = "俯视角角色方向输入")]
    public class TopdownVector2DComposite : InputBindingComposite<Vector2>
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

        [Tooltip("状态切换的延时")]
        public float switchDelay = 0.2f;

        DirectionStates m_LastStates;
        float m_LastStateChanged;

        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            float time = Time.time;

            bool upPressed = context.ReadValueAsButton(up);
            bool downPressed = context.ReadValueAsButton(down);
            bool leftPressed = context.ReadValueAsButton(left);
            bool rightPressed = context.ReadValueAsButton(right);

            EDirectionState horizontalState = DirectionStatesExtensions.GetDirectionState(rightPressed, leftPressed);
            EDirectionState verticalState = DirectionStatesExtensions.GetDirectionState(upPressed, downPressed);
            DirectionStates states = new(horizontalState, verticalState);

            if (states.IsNone() || m_LastStates.IsNone()
            || !DirectionStates.Equals(states, m_LastStates) && (time - m_LastStateChanged) >= switchDelay)
            {
                m_LastStates = states;
                m_LastStateChanged = time;
            }

            return m_LastStates.GetVector();
        }
    }
}