using UnityEngine;
using Mmang.Util;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Game
{
    public class GameInputManager : SingletonMono<GameInputManager>
    {
        public enum EControlMode { None, Mouse, Gamepad, Keyboard }
        public static event System.Action<EControlMode> OnInputDeviceChanged;
        public static EControlMode CurrentControlScheme { get; private set; }

        private GameInput m_Input;
        public InputActionAsset inputActions;
        private InputUser m_User;

        

        protected override void OnAwake()
        {
            base.OnAwake();
            m_Input = new();
        }

        private void OnEnable()
        {
            m_Input.Enable();
            m_User = InputUser.CreateUserWithoutPairedDevices();
            m_User.AssociateActionsWithUser(inputActions.actionMaps[0]); // 确保至少有一个 action map
            ++InputUser.listenForUnpairedDeviceActivity;
            InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
            m_User.UnpairDevices();
        }

        private void OnDisable()
        {
            m_Input.Disable();
            InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
            if (InputUser.listenForUnpairedDeviceActivity > 0)
                --InputUser.listenForUnpairedDeviceActivity;
            m_User.UnpairDevicesAndRemoveUser();
        }

        private EControlMode GetControlModeDeviceUsed(InputDevice device)
        {
            if (device is Pointer)
                return EControlMode.Mouse;
            if (device is Gamepad)
                return EControlMode.Gamepad;
            return EControlMode.Keyboard;
        }

        private void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
        {
            var device = control.device;
            
            var mode = GetControlModeDeviceUsed(device);
            
            if (CurrentControlScheme != mode)
            {
                CurrentControlScheme = mode;
                OnInputDeviceChanged?.Invoke(mode);
                //Debug.Log(mode);
                m_User.UnpairDevices();
                InputUser.PerformPairingWithDevice(device, m_User);
            }

            /*
            if ((CurrentControlScheme == "KeyboardMouse") && ((device is Pointer) || (device is Keyboard)))
            {
                InputUser.PerformPairingWithDevice(device, m_User);
                OnInputDeviceChanged?.Invoke("KeyboardMouse");
                SetUserControlScheme("KeyboardMouse");
                return;
            }

            if (device is Gamepad)
            {
                OnInputDeviceChanged?.Invoke("Gamepad");
                CurrentControlScheme = "Gamepad";
                SetUserControlScheme("Gamepad");
            }
            else if ((device is Keyboard) || (device is Pointer))
            {
                OnInputDeviceChanged?.Invoke("KeyboardMouse");
                CurrentControlScheme = "KeyboardMouse";
                SetUserControlScheme("KeyboardMouse");
            }
            else return;

            m_User.UnpairDevices();
            InputUser.PerformPairingWithDevice(device, m_User);
            */
        }

        private void SetUserControlScheme(string scheme)
        {
            foreach (var sch in inputActions.controlSchemes)
            {
                if (sch.name == scheme)
                {
                    m_User.ActivateControlScheme(sch);
                    break;
                }
            }
            Debug.Log(scheme);
        }

        public static Vector2 GetDirection()
        {
            return Instance.m_Input.Player.Move.ReadValue<Vector2>();
        }

        public static Vector2 GetLookDirection()
        {
            return Instance.m_Input.Player.Look.ReadValue<Vector2>();
        }

        public static bool GetSkillPressed()
        {
            return Instance.m_Input.Player.Skill.IsPressed();
        }

        public static bool GetSuicidePressed()
        {
            return Instance.m_Input.Player.Suicide.IsPressed();
        }
    }
}