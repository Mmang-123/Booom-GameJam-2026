using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace Mmang.InputExtensions
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    public static class InputSystemRegister
    {
        static InputSystemRegister()
        {
            InputSystem.RegisterBindingComposite<TopdownVector2DComposite>();
            InputSystem.RegisterBindingComposite<PlatformerVector2DComposite>();
        }
    }
#endif
}