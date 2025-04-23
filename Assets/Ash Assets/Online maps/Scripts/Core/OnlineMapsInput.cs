/*         INFINITY CODE         */
/*   https://infinity-code.com   */

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
#define USE_NEW_INPUT
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
#define USE_LEGACY_INPUT
#endif

using System;
using UnityEngine;
using Touch = UnityEngine.Touch;

#if USE_NEW_INPUT
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using ET = UnityEngine.InputSystem.EnhancedTouch;
#endif

/// <summary>
/// Class that allows you to get input using old and new input systems.
/// </summary>
public static class OnlineMapsInput
{
    /// <summary>
    /// Allows you to change the returned status of a key by KeyCode.
    /// </summary>
    public static Func<KeyCode, bool> OnGetKeyByKeyCode;
    
    /// <summary>
    /// Allows you to change the returned status of a key by name.
    /// </summary>
    public static Func<string, bool> OnGetKeyByName;
    
    /// <summary>
    /// Allows you to change the returned status of a mouse button.
    /// </summary>
    public static Func<int, bool> OnGetMouseButton;

#if USE_NEW_INPUT
    private static Vector2 lastTouchPosition;
#endif

    /// <summary>
    /// Returns the position of the mouse cursor.
    /// </summary>
    public static Vector2 mousePosition
    {
        get
        {
#if USE_NEW_INPUT
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            if (touchSupported)
            {
                if (touchCount > 0) return lastTouchPosition = GetTouch(0).position;
                return lastTouchPosition;
            }

            return Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }
    }

    /// <summary>
    /// Number of touches. Guaranteed not to change throughout the frame.
    /// </summary>
    public static int touchCount
    {
        get
        {
#if USE_NEW_INPUT
            if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();
            return ET.Touch.activeTouches.Count;
#else
            return Input.touchCount;
#endif
        }
    }

    /// <summary>
    /// Returns whether the device on which application is currently running supports touch input.
    /// </summary>
    public static bool touchSupported
    {
        get
        {
#if USE_NEW_INPUT
            return Touchscreen.current != null;
#else
            return Input.touchSupported;
#endif
        }
    }

    /// <summary>
    /// Returns list of objects representing status of all touches during last frame.
    /// </summary>
    public static Touch[] touches
    {
        get
        {
#if USE_NEW_INPUT
            if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();
            ReadOnlyArray<ET.Touch> _touches = ET.Touch.activeTouches;
            Touch[] touches = new Touch[_touches.Count];
            for (int i = 0; i < touches.Length; i++)
            {
                touches[i] = ConvertTouch(_touches[i]);
            }
            return touches;
#else
            return Input.touches;
#endif
        }
    }

#if USE_NEW_INPUT
    private static Touch ConvertTouch(ET.Touch touch)
    {
        Touch t = new Touch
        {
            position = touch.screenPosition,
            rawPosition = touch.screenPosition,
            deltaPosition = touch.delta,
            fingerId = touch.touchId,
            tapCount = touch.tapCount,
            deltaTime = Time.deltaTime,
            pressure = touch.pressure,
            radius = touch.radius.magnitude
        };
        
        TouchPhase touchPhase = touch.phase;

        t.phase = touchPhase switch
        {
            TouchPhase.Began => UnityEngine.TouchPhase.Began,
            TouchPhase.Moved => UnityEngine.TouchPhase.Moved,
            TouchPhase.Stationary => UnityEngine.TouchPhase.Stationary,
            TouchPhase.Ended => UnityEngine.TouchPhase.Ended,
            TouchPhase.Canceled => UnityEngine.TouchPhase.Canceled,
            _ => t.phase
        };

        return t;
    }
#endif

    /// <summary>
    /// Returns the value of the virtual axis identified by axisName.
    /// </summary>
    /// <param name="axisName">Name of the axis</param>
    /// <returns>Value of the axis</returns>
    public static float GetAxis(string axisName)
    {
#if USE_NEW_INPUT
        if (axisName == "Mouse ScrollWheel") return Mouse.current.scroll.ReadValue().y;
        if (axisName == "Horizontal")
        {
            if (GetKey(KeyCode.A) || GetKey(KeyCode.LeftArrow)) return -1;
            if (GetKey(KeyCode.D) || GetKey(KeyCode.RightArrow)) return 1;
            return 0;
        }
        if (axisName == "Vertical")
        {
            if (GetKey(KeyCode.S) || GetKey(KeyCode.DownArrow)) return -1;
            if (GetKey(KeyCode.W) || GetKey(KeyCode.UpArrow)) return 1;
            return 0;
        }
        
        return 0;
#else
        return Input.GetAxis(axisName);
#endif
    }

    /// <summary>
    /// Returns true while the user holds down the key identified by keyCode.
    /// </summary>
    /// <param name="keyCode">Key code</param>
    /// <returns>True - the key is pressed, False - the key is not pressed</returns>
    public static bool GetKey(KeyCode keyCode)
    {
        if (OnGetKeyByKeyCode != null) return OnGetKeyByKeyCode(keyCode);

#if USE_NEW_INPUT
        Key key = KeyCodeToKey(keyCode);
        if (key == Key.None)
        {
            Debug.LogWarning($"Key {keyCode} is not added to the conversion list.");
            return false;
        }
        KeyControl control = Keyboard.current[key];
        return control != null && control.isPressed;
#else
        return Input.GetKey(keyCode);
#endif
    }

    /// <summary>
    /// Returns true during the frame the user starts pressing down the key identified by name.
    /// </summary>
    /// <param name="name">Name of the key</param>
    /// <returns>True - the key is pressed, False - the key is not pressed</returns>
    public static bool GetKey(string name)
    {
        if (OnGetKeyByName != null) return OnGetKeyByName(name);
#if USE_NEW_INPUT
        KeyControl control = (KeyControl) Keyboard.current[name];
        return control != null && control.isPressed;
#else
        return Input.GetKey(name);
#endif
    }
    
    /// <summary>
    /// Returns true during the frame the user starts pressing down the key identified by keyCode.
    /// </summary>
    /// <param name="keyCode">Key code</param>
    /// <returns>True - the key is pressed, False - the key is not pressed</returns>
    public static bool GetKeyDown(KeyCode keyCode)
    {
#if USE_NEW_INPUT
        KeyControl control = Keyboard.current[KeyCodeToKey(keyCode)];
        return control != null && control.wasPressedThisFrame;
#else
        return Input.GetKeyDown(keyCode);
#endif
    }

    /// <summary>
    /// Returns true during the frame the user releases the key identified by keyCode.
    /// </summary>
    /// <param name="keyCode">Key code</param>
    /// <returns>True - the key is released, False - the key is not released</returns>
    public static bool GetKeyUp(KeyCode keyCode)
    {
#if USE_NEW_INPUT
        KeyControl control = Keyboard.current[KeyCodeToKey(keyCode)];
        return control != null && control.wasReleasedThisFrame;
#else
        return Input.GetKeyUp(keyCode);
#endif
    }

    /// <summary>
    /// Returns whether the given mouse button is held down.
    /// </summary>
    /// <param name="button">Button index</param>
    /// <returns>True - the button is pressed, False - the button is not pressed</returns>
    public static bool GetMouseButton(int button)
    {
        if (OnGetMouseButton != null) return OnGetMouseButton(button);

#if USE_NEW_INPUT
        Mouse mouse = Mouse.current;
        if (mouse == null) return touchSupported && touchCount == button + 1;
        
        InputControl control;
        
        if (button == 0) control = mouse.leftButton;
        else if (button == 1) control = mouse.rightButton;
        else if (button == 2) control = mouse.middleButton;
        else return false;
        
        return control != null && control.IsPressed();
#else
        return Input.GetMouseButton(button);
#endif
    }

    /// <summary>
    /// Returns whether the given mouse button was pressed this frame.
    /// </summary>
    /// <param name="button">Button index</param>
    /// <returns>True - the button is pressed, False - the button is not pressed</returns>
    public static bool GetMouseButtonDown(int button)
    {
#if USE_NEW_INPUT
        Mouse mouse = Mouse.current;
        if (button == 0) return mouse.leftButton.wasPressedThisFrame;
        if (button == 1) return mouse.rightButton.wasPressedThisFrame;
        if (button == 2) return mouse.middleButton.wasPressedThisFrame;
        return false;
#else
        return Input.GetMouseButtonDown(button);
#endif
    }
    
    /// <summary>
    /// Returns whether the given mouse button was released this frame.
    /// </summary>
    /// <param name="button">Button index</param>
    /// <returns>True - the button is released, False - the button is not released</returns>
    public static bool GetMouseButtonUp(int button)
    {
#if USE_NEW_INPUT
        Mouse mouse = Mouse.current;
        if (button == 0) return mouse.leftButton.wasReleasedThisFrame;
        if (button == 1) return mouse.rightButton.wasReleasedThisFrame;
        if (button == 2) return mouse.middleButton.wasReleasedThisFrame;
        return false;
#else
        return Input.GetMouseButtonUp(button);
#endif
    }

    /// <summary>
    /// Returns the touch for a given index.
    /// </summary>
    /// <param name="index">Touch index</param>
    /// <returns>Touch</returns>
    public static Touch GetTouch(int index)
    {
#if USE_NEW_INPUT
        if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();
        
        ReadOnlyArray<ET.Touch> _touches = ET.Touch.activeTouches;
        if (index < 0 || index >= _touches.Count) return new Touch();
        return ConvertTouch(_touches[index]);
#else
        return Input.GetTouch(index);
#endif
    }

#if USE_NEW_INPUT
    private static Key KeyCodeToKey(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.A: // CHARS
                return Key.A;
            case KeyCode.B:
                return Key.B;
            case KeyCode.C:
                return Key.C;
            case KeyCode.D:
                return Key.D;
            case KeyCode.E:
                return Key.E;
            case KeyCode.F:
                return Key.F;
            case KeyCode.G:
                return Key.G;
            case KeyCode.H:
                return Key.H;
            case KeyCode.I:
                return Key.I;
            case KeyCode.J:
                return Key.J;
            case KeyCode.K:
                return Key.K;
            case KeyCode.L:
                return Key.L;
            case KeyCode.M:
                return Key.M;
            case KeyCode.N:
                return Key.N;
            case KeyCode.O:
                return Key.O;
            case KeyCode.P:
                return Key.P;
            case KeyCode.Q:
                return Key.Q;
            case KeyCode.R:
                return Key.R;
            case KeyCode.S:
                return Key.S;
            case KeyCode.T:
                return Key.T;
            case KeyCode.U:
                return Key.U;
            case KeyCode.V:
                return Key.V;
            case KeyCode.W:
                return Key.W;
            case KeyCode.X:
                return Key.X;
            case KeyCode.Y:
                return Key.Y;
            case KeyCode.Z:
                return Key.Z;
            case KeyCode.Alpha0: // Digits
                return Key.Digit0;
            case KeyCode.Alpha1:
                return Key.Digit1;
            case KeyCode.Alpha2:
                return Key.Digit2;
            case KeyCode.Alpha3:
                return Key.Digit3;
            case KeyCode.Alpha4:
                return Key.Digit4;
            case KeyCode.Alpha5:
                return Key.Digit5;
            case KeyCode.Alpha6:
                return Key.Digit6;
            case KeyCode.Alpha7:
                return Key.Digit7;
            case KeyCode.Alpha8:
                return Key.Digit8;
            case KeyCode.Alpha9:
                return Key.Digit9;
            case KeyCode.Minus:
                return Key.Minus;
            case KeyCode.Equals:
                return Key.Equals;
            case KeyCode.Keypad0: // Keypad
                return Key.Numpad0;
            case KeyCode.Keypad1:
                return Key.Numpad1;
            case KeyCode.Keypad2:
                return Key.Numpad2;
            case KeyCode.Keypad3:
                return Key.Numpad3;
            case KeyCode.Keypad4:
                return Key.Numpad4;
            case KeyCode.Keypad5:
                return Key.Numpad5;
            case KeyCode.Keypad6:
                return Key.Numpad6;
            case KeyCode.Keypad7:
                return Key.Numpad7;
            case KeyCode.Keypad8:
                return Key.Numpad8;
            case KeyCode.Keypad9:
                return Key.Numpad9;
            case KeyCode.KeypadDivide:
                return Key.NumpadDivide;
            case KeyCode.KeypadMultiply:
                return Key.NumpadMultiply;
            case KeyCode.KeypadMinus:
                return Key.NumpadMinus;
            case KeyCode.KeypadPlus:
                return Key.NumpadPlus;
            case KeyCode.KeypadPeriod:
                return Key.NumpadPeriod;
            case KeyCode.KeypadEquals:
                return Key.NumpadEquals;
            case KeyCode.KeypadEnter:
                return Key.NumpadEnter;
            case KeyCode.F1: // F1 - F12
                return Key.F1;
            case KeyCode.F2:
                return Key.F2;
            case KeyCode.F3:
                return Key.F3;
            case KeyCode.F4:
                return Key.F4;
            case KeyCode.F5:
                return Key.F5;
            case KeyCode.F6:
                return Key.F6;
            case KeyCode.F7:
                return Key.F7;
            case KeyCode.F8:
                return Key.F8;
            case KeyCode.F9:
                return Key.F9;
            case KeyCode.F10:
                return Key.F10;
            case KeyCode.F11:
                return Key.F11;
            case KeyCode.F12:
                return Key.F12;
            case KeyCode.LeftArrow: // Arrows
                return Key.LeftArrow;
            case KeyCode.UpArrow:
                return Key.UpArrow;
            case KeyCode.RightArrow:
                return Key.RightArrow;
            case KeyCode.DownArrow:
                return Key.DownArrow;
            case KeyCode.LeftControl: // Modifiers
                return Key.LeftCtrl;
            case KeyCode.LeftAlt:
                return Key.LeftAlt;
            case KeyCode.AltGr:
                return Key.AltGr;
            case KeyCode.LeftShift:
                return Key.LeftShift;
            case KeyCode.LeftCommand:
                return Key.LeftCommand;
            case KeyCode.RightControl:
                return Key.RightCtrl;
            case KeyCode.RightAlt:
                return Key.RightAlt;
            case KeyCode.RightShift:
                return Key.RightShift;
            case KeyCode.RightCommand:
                return Key.RightCommand;
            case KeyCode.Menu:
                return Key.ContextMenu;
            case KeyCode.Quote: // Special
                return Key.Quote;
            case KeyCode.BackQuote:
                return Key.Backquote;
            case KeyCode.Space:
                return Key.Space;
            case KeyCode.Return:
                return Key.Enter;
            case KeyCode.Escape:
                return Key.Escape;
            case KeyCode.Tab:
                return Key.Tab;
            case KeyCode.Semicolon:
                return Key.Semicolon;
            case KeyCode.Comma:
                return Key.Comma;
            case KeyCode.Period:
                return Key.Period;
            case KeyCode.Slash:
                return Key.Slash;
            case KeyCode.Backslash:
                return Key.Backslash;
            case KeyCode.LeftBracket:
                return Key.LeftBracket;
            case KeyCode.RightBracket:
                return Key.RightBracket;
            case KeyCode.Backspace:
                return Key.Backspace;
            case KeyCode.PageUp:
                return Key.PageUp;
            case KeyCode.PageDown:
                return Key.PageDown;
            case KeyCode.Home:
                return Key.Home;
            case KeyCode.End:
                return Key.End;
            case KeyCode.Insert:
                return Key.Insert;
            case KeyCode.Delete:
                return Key.Delete;
            case KeyCode.CapsLock:
                return Key.CapsLock;
            case KeyCode.Numlock:
                return Key.NumLock;
            case KeyCode.Print:
                return Key.PrintScreen;
            case KeyCode.ScrollLock:
                return Key.ScrollLock;
            case KeyCode.Pause:
                return Key.Pause;
            default:
                return Key.None;
        }
    }
#endif
}