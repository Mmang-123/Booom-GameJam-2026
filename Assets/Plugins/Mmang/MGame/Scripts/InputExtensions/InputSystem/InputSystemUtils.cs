using UnityEngine;
using UnityEngine.InputSystem;

namespace Mmang.InputExtensions
{
    
    public static class InputSystemUtils
    {
        public static InputAction CreateCustomVector2DInputAction(
            string compositeName,
            string upBinding = "<Keyboard>/w",
            string downBinding = "<Keyboard>/s",
            string leftBinding = "<Keyboard>/a",
            string rightBinding = "<Keyboard>/d"
        )
        {
            InputAction action = new("Vector2DInput", InputActionType.Value);

            action.AddCompositeBinding(compositeName)
                .With("Up", upBinding)
                .With("Down", downBinding)
                .With("Left", leftBinding)
                .With("Right", rightBinding);

            return action;
        }


        public static InputAction CreateVector2DInputAction(
            string upBinding = "<Keyboard>/w",
            string downBinding = "<Keyboard>/s",
            string leftBinding = "<Keyboard>/a",
            string rightBinding = "<Keyboard>/d"
        )
        {
            return CreateCustomVector2DInputAction("2DVector", upBinding, downBinding, leftBinding, rightBinding);
        }

        public static InputAction CreateTopdownVector2DInputAction(float switchDelay = 0.2f,
            string upBinding = "<Keyboard>/w",
            string downBinding = "<Keyboard>/s",
            string leftBinding = "<Keyboard>/a",
            string rightBinding = "<Keyboard>/d"
        )
        {
            return CreateCustomVector2DInputAction($"TopdownVector2D(switchDelay={switchDelay})", upBinding, downBinding, leftBinding, rightBinding);
        }

        public static InputAction CreatePlatformerVector2DInputAction(
            string upBinding = "<Keyboard>/w",
            string downBinding = "<Keyboard>/s",
            string leftBinding = "<Keyboard>/a",
            string rightBinding = "<Keyboard>/d"
        )
        {
            return CreateCustomVector2DInputAction("PlatformerVector2D", upBinding, downBinding, leftBinding, rightBinding);
        }
    }
}