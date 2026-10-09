using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Ascension
{
    /// <summary>
    /// Single input façade. Works with the new Input System or the legacy Input Manager,
    /// so gameplay code never touches either API directly.
    /// </summary>
    public static class InputReader
    {
        public static Vector2 Move
        {
            get
            {
                Vector2 v = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                if (k == null) return v;
                if (k.wKey.isPressed || k.upArrowKey.isPressed) v.y += 1;
                if (k.sKey.isPressed || k.downArrowKey.isPressed) v.y -= 1;
                if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1;
                if (k.aKey.isPressed || k.leftArrowKey.isPressed) v.x -= 1;
#else
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v.y += 1;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v.y -= 1;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) v.x += 1;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) v.x -= 1;
#endif
                return v.normalized;
            }
        }

        public static Vector2 MouseScreen
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
                return Input.mousePosition;
#endif
            }
        }

        public static bool MeleeHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return (Mouse.current != null && Mouse.current.leftButton.isPressed) || (Keyboard.current != null && Keyboard.current.jKey.isPressed);
#else
                return Input.GetMouseButton(0) || Input.GetKey(KeyCode.J);
#endif
            }
        }

        public static bool RangedHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return (Mouse.current != null && Mouse.current.rightButton.isPressed) || (Keyboard.current != null && Keyboard.current.kKey.isPressed);
#else
                return Input.GetMouseButton(1) || Input.GetKey(KeyCode.K);
#endif
            }
        }

        public static bool DashPressed => KeyDown(KeyCode.Space);
        public static bool InteractPressed => KeyDown(KeyCode.E);
        public static bool SpecialPressed => KeyDown(KeyCode.Q);
        public static bool AcceptPromptPressed => KeyDown(KeyCode.F);
        public static bool CancelPressed => KeyDown(KeyCode.Escape);

        public static bool NumberPressed(int n) => KeyDown(KeyCode.Alpha1 + (n - 1));

        public static bool KeyDown(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k == null) return false;
            Key key = code switch
            {
                KeyCode.Space => Key.Space,
                KeyCode.E => Key.E,
                KeyCode.Q => Key.Q,
                KeyCode.F => Key.F,
                KeyCode.Escape => Key.Escape,
                KeyCode.Alpha1 => Key.Digit1,
                KeyCode.Alpha2 => Key.Digit2,
                KeyCode.Alpha3 => Key.Digit3,
                KeyCode.Alpha4 => Key.Digit4,
                KeyCode.R => Key.R,
                _ => Key.None
            };
            return key != Key.None && k[key].wasPressedThisFrame;
#else
            return Input.GetKeyDown(code);
#endif
        }
    }
}
