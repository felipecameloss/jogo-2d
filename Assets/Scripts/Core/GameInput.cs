using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Centraliza a leitura de input. Funciona tanto com o Input System novo
// quanto com o Input Manager antigo, dependendo do que o projeto usar.
public static class GameInput
{
    public static Vector2 Move()
    {
#if ENABLE_INPUT_SYSTEM
        Vector2 v = Vector2.zero;
        var k = Keyboard.current;
        if (k != null)
        {
            if (k.aKey.isPressed || k.leftArrowKey.isPressed) v.x -= 1;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1;
            if (k.sKey.isPressed || k.downArrowKey.isPressed) v.y -= 1;
            if (k.wKey.isPressed || k.upArrowKey.isPressed) v.y += 1;
        }
        var g = Gamepad.current;
        if (g != null && v == Vector2.zero) v = g.leftStick.ReadValue();
        return Vector2.ClampMagnitude(v, 1f);
#else
        return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
#endif
    }

    public static bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        var g = Gamepad.current;
        return (k != null && k.fKey.wasPressedThisFrame) || (g != null && g.buttonNorth.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton3);
#endif
    }

    public static bool CollectHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        var g = Gamepad.current;
        return (k != null && k.eKey.isPressed) || (g != null && g.buttonWest.isPressed);
#else
        return Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton2);
#endif
    }

    public static bool PausePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        var g = Gamepad.current;
        return (k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame))
            || (g != null && g.startButton.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
    }

    public static bool DashPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        var g = Gamepad.current;
        return (k != null && k.leftShiftKey.wasPressedThisFrame)
            || (g != null && (g.buttonSouth.wasPressedThisFrame || g.rightShoulder.wasPressedThisFrame));
#else
        return Input.GetKeyDown(KeyCode.LeftShift);
#endif
    }

    // Atalho de teste (só funciona no Editor): N pula para o próximo período (dia/noite).
    public static bool DebugSkipPeriodPressed()
    {
#if UNITY_EDITOR
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && k.nKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.N);
#endif
#else
        return false;
#endif
    }

    // Retorna 0, 1 ou 2 se as teclas 1, 2 ou 3 foram apertadas neste frame; senão -1.
    public static int OptionPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return -1;
        if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) return 0;
        if (k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) return 1;
        if (k.digit3Key.wasPressedThisFrame || k.numpad3Key.wasPressedThisFrame) return 2;
#else
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 2;
#endif
        return -1;
    }
}
