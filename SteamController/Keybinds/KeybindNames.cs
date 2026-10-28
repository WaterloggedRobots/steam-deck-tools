using SteamController.ProfilesSettings;

namespace SteamController.Keybinds
{
    public enum ControllerMode
    {
        Desktop,
        X360,
        DS4
    }

    /// Human readable names shown in the keybind editor
    internal static class KeybindNames
    {
        public static String Button(DeckButton button)
        {
            switch (button)
            {
                case DeckButton.DPadUp: return "D-pad Up";
                case DeckButton.DPadDown: return "D-pad Down";
                case DeckButton.DPadLeft: return "D-pad Left";
                case DeckButton.DPadRight: return "D-pad Right";
                case DeckButton.LeftStick: return "Left Stick Press (L3)";
                case DeckButton.RightStick: return "Right Stick Press (R3)";
                case DeckButton.LeftPad: return "Left Trackpad Press";
                case DeckButton.RightPad: return "Right Trackpad Press";
                case DeckButton.Menu: return "View (two squares)";
                case DeckButton.Options: return "Options (three lines)";
                case DeckButton.Steam: return "STEAM";
                case DeckButton.QuickAccess: return "Quick Access (3 dots)";
                default: return button.ToString();
            }
        }

        public static String Layer(KeybindLayer layer)
        {
            switch (layer)
            {
                case KeybindLayer.Steam: return "STEAM + button";
                case KeybindLayer.QuickAccess: return "3 dots + button";
                default: return "Button alone";
            }
        }

        public static String Mode(ControllerMode mode)
        {
            switch (mode)
            {
                case ControllerMode.X360: return "X360 controller";
                case ControllerMode.DS4: return "DS4 controller";
                default: return "Desktop (mouse & keyboard)";
            }
        }

        public static String ActionType(KeybindActionType type)
        {
            switch (type)
            {
                case KeybindActionType.Default: return "Default (built-in)";
                case KeybindActionType.Disabled: return "Do nothing";
                case KeybindActionType.Keyboard: return "Keyboard key";
                case KeybindActionType.Mouse: return "Mouse button";
                case KeybindActionType.Xbox360: return "X360 button (X360 mode only)";
                case KeybindActionType.DS4: return "DS4 button (DS4 mode only)";
                case KeybindActionType.Special: return "Windows action";
                default: return type.ToString();
            }
        }

        public static String Special(SpecialAction special)
        {
            switch (special)
            {
                case SpecialAction.None: return "None";
                case SpecialAction.OnScreenKeyboard: return "Toggle on-screen keyboard";
                case SpecialAction.Screenshot: return "Screenshot (WIN+PrtSc)";
                case SpecialAction.ToggleMagnifier: return "Toggle magnifier";
                case SpecialAction.TaskManager: return "Task Manager";
                case SpecialAction.TaskView: return "Task view (WIN+TAB)";
                case SpecialAction.ShowDesktop: return "Show desktop (WIN+D)";
                case SpecialAction.ToggleFullscreen: return "Fullscreen (F11)";
                case SpecialAction.CloseApp: return "Close app (ALT+F4)";
                case SpecialAction.GameBar: return "Xbox Game Bar (WIN+G)";
                case SpecialAction.BrightnessUp: return "Brightness up";
                case SpecialAction.BrightnessDown: return "Brightness down";
                case SpecialAction.NextControllerProfile: return "Next controller mode";
                case SpecialAction.BackToDesktopMode: return "Back to desktop mode";
                case SpecialAction.NextKeybindProfile: return "Next keybind profile";
                default: return special.ToString();
            }
        }

        public static String Key(VirtualKeyCode key)
        {
            switch (key)
            {
                case VirtualKeyCode.None: return "None";
                case VirtualKeyCode.RETURN: return "Enter";
                case VirtualKeyCode.ESCAPE: return "Esc";
                case VirtualKeyCode.BACKSPACE: return "Backspace";
                case VirtualKeyCode.CONTROL: return "Ctrl";
                case VirtualKeyCode.SHIFT: return "Shift";
                case VirtualKeyCode.ALT: return "Alt";
                case VirtualKeyCode.LWIN: return "Win";
                case VirtualKeyCode.RWIN: return "Right Win";
                case VirtualKeyCode.PAGE_UP: return "PgUp";
                case VirtualKeyCode.PAGE_DOWN: return "PgDn";
                case VirtualKeyCode.PRINT_SCREEN: return "PrtSc";
                case VirtualKeyCode.LEFT: return "Left";
                case VirtualKeyCode.RIGHT: return "Right";
                case VirtualKeyCode.UP: return "Up";
                case VirtualKeyCode.DOWN: return "Down";
                case VirtualKeyCode.MULTIPLY: return "Num *";
                case VirtualKeyCode.ADD: return "Num +";
                case VirtualKeyCode.SUBTRACT: return "Num -";
                case VirtualKeyCode.DIVIDE: return "Num /";
                case VirtualKeyCode.DECIMAL: return "Num .";
            }

            var name = key.ToString();
            if (name.StartsWith("VK_"))
                return name.Substring(3);
            if (name.StartsWith("NUMPAD"))
                return "Num " + name.Substring(6);

            // VOLUME_UP => Volume Up
            return String.Join(" ", name.Split('_').Select(
                (part) => part.Length > 1 ? part.Substring(0, 1) + part.Substring(1).ToLower() : part));
        }

        public static String Mouse(Devices.MouseController.Button button)
        {
            switch (button)
            {
                case Devices.MouseController.Button.Left: return "Left click";
                case Devices.MouseController.Button.Right: return "Right click";
                case Devices.MouseController.Button.Middle: return "Middle click";
                case Devices.MouseController.Button.X: return "Mouse Back (X1)";
                case Devices.MouseController.Button.Y: return "Mouse Forward (X2)";
                default: return button.ToString();
            }
        }

        public static String X360(VirtualX360Code code)
        {
            if (code == VirtualX360Code.None)
                return "None";
            return "X360 " + code.ToString().Replace("X360_", "");
        }

        public static String DS4(VirtualDS4Code code)
        {
            if (code == VirtualDS4Code.None)
                return "None";
            return "DS4 " + code.ToString().Replace("DS4_", "").Replace("STICK_", "Stick ");
        }

        public static String Action(KeybindAction? action)
        {
            if (action is null)
                return "Default";

            switch (action.Type)
            {
                case KeybindActionType.Disabled:
                    return "Nothing";

                case KeybindActionType.Keyboard:
                    var parts = new List<String>();
                    if (action.Ctrl) parts.Add("Ctrl");
                    if (action.Shift) parts.Add("Shift");
                    if (action.Alt) parts.Add("Alt");
                    if (action.Win) parts.Add("Win");
                    if (action.Key != VirtualKeyCode.None || parts.Count == 0)
                        parts.Add(Key(action.Key));
                    return String.Join("+", parts);

                case KeybindActionType.Mouse:
                    return Mouse(action.Mouse);

                case KeybindActionType.Xbox360:
                    return X360(action.X360);

                case KeybindActionType.DS4:
                    return DS4(action.DS4);

                case KeybindActionType.Special:
                    return Special(action.Special);

                default:
                    return "Default";
            }
        }
    }
}
