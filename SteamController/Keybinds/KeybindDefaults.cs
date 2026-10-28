using SteamController.ProfilesSettings;

namespace SteamController.Keybinds
{
    /// Describes what the built-in profiles do with a button (see docs/shortcuts.md)
    internal static class KeybindDefaults
    {
        private const String Nothing = "—";

        public static String Describe(ControllerMode mode, KeybindLayer layer, DeckButton button)
        {
            switch (layer)
            {
                case KeybindLayer.Steam:
                    return Steam(button);
                case KeybindLayer.QuickAccess:
                    return button == DeckButton.Steam ? "Task Manager" : Nothing;
            }

            switch (button)
            {
                case DeckButton.Steam:
                    return mode == ControllerMode.X360 ? "X360 Guide" : mode == ControllerMode.DS4 ? "DS4 PS" : Nothing;
                case DeckButton.QuickAccess:
                    return "Hold for shortcuts";
                case DeckButton.L4:
                case DeckButton.L5:
                case DeckButton.R4:
                case DeckButton.R5:
                    return BackPanel(mode, button);
                case DeckButton.LeftPad:
                    return mode == ControllerMode.DS4 ? "DS4 Touchpad click" : "Right click";
                case DeckButton.RightPad:
                    return mode == ControllerMode.DS4 ? "DS4 Touchpad click" : "Left click";
            }

            switch (mode)
            {
                case ControllerMode.X360:
                    return X360(button);
                case ControllerMode.DS4:
                    return DS4(button);
                default:
                    return Desktop(button);
            }
        }

        private static String Desktop(DeckButton button)
        {
            switch (button)
            {
                case DeckButton.A: return "Enter";
                case DeckButton.B: return "Backspace";
                case DeckButton.DPadUp: return "Up";
                case DeckButton.DPadDown: return "Down";
                case DeckButton.DPadLeft: return "Left";
                case DeckButton.DPadRight: return "Right";
                case DeckButton.L2: return "Right click";
                case DeckButton.R2: return "Left click";
                case DeckButton.Options: return "Hold: next mode";
                default: return Nothing;
            }
        }

        private static String X360(DeckButton button)
        {
            switch (button)
            {
                case DeckButton.DPadUp: return "X360 Up";
                case DeckButton.DPadDown: return "X360 Down";
                case DeckButton.DPadLeft: return "X360 Left";
                case DeckButton.DPadRight: return "X360 Right";
                case DeckButton.L1: return "X360 LB";
                case DeckButton.R1: return "X360 RB";
                case DeckButton.L2: return "X360 LT";
                case DeckButton.R2: return "X360 RT";
                case DeckButton.LeftStick: return "X360 LS";
                case DeckButton.RightStick: return "X360 RS";
                case DeckButton.Menu: return "X360 Back";
                case DeckButton.Options: return "X360 Start";
                default: return "X360 " + button.ToString();
            }
        }

        private static String DS4(DeckButton button)
        {
            switch (button)
            {
                case DeckButton.A: return "DS4 Cross";
                case DeckButton.B: return "DS4 Circle";
                case DeckButton.X: return "DS4 Square";
                case DeckButton.Y: return "DS4 Triangle";
                case DeckButton.DPadUp: return "DS4 Up";
                case DeckButton.DPadDown: return "DS4 Down";
                case DeckButton.DPadLeft: return "DS4 Left";
                case DeckButton.DPadRight: return "DS4 Right";
                case DeckButton.LeftStick: return "DS4 L3";
                case DeckButton.RightStick: return "DS4 R3";
                case DeckButton.Menu: return "DS4 Share";
                case DeckButton.Options: return "DS4 Options";
                default: return "DS4 " + button.ToString();
            }
        }

        private static String Steam(DeckButton button)
        {
            switch (button)
            {
                case DeckButton.A: return "Enter";
                case DeckButton.B: return "Hold: Alt+F4 / kill";
                case DeckButton.X: return "On-screen keyboard";
                case DeckButton.L1: return "Magnifier";
                case DeckButton.R1: return "Screenshot";
                case DeckButton.L2: return "Right click";
                case DeckButton.R2: return "Left click";
                case DeckButton.DPadUp: return "Ctrl+Alt+U";
                case DeckButton.DPadDown: return "Tab";
                case DeckButton.DPadLeft: return "Esc";
                case DeckButton.DPadRight: return "Enter";
                case DeckButton.RightStick: return "Show desktop";
                case DeckButton.LeftPad: return "Right click";
                case DeckButton.RightPad: return "Left click";
                case DeckButton.Menu: return "F11";
                case DeckButton.Options: return "Task view";
                case DeckButton.QuickAccess: return "Task Manager";
                default: return Nothing;
            }
        }

        private static String BackPanel(ControllerMode mode, DeckButton button)
        {
            BackPanelSettings settings = ProfilesSettings.DesktopPanelSettings.Default;
            if (mode == ControllerMode.X360)
                settings = ProfilesSettings.X360BackPanelSettings.Default;
            else if (mode == ControllerMode.DS4)
                settings = ProfilesSettings.DS4BackPanelSettings.Default;

            VirtualKeyCode key;
            switch (button)
            {
                case DeckButton.L4: key = settings.L4_KEY; break;
                case DeckButton.L5: key = settings.L5_KEY; break;
                case DeckButton.R4: key = settings.R4_KEY; break;
                default: key = settings.R5_KEY; break;
            }

            if (key != VirtualKeyCode.None)
                return KeybindNames.Key(key);

            if (settings is X360BackPanelSettings x360)
            {
                var code = button == DeckButton.L4 ? x360.L4_X360 : button == DeckButton.L5 ? x360.L5_X360 : button == DeckButton.R4 ? x360.R4_X360 : x360.R5_X360;
                if (code != VirtualX360Code.None)
                    return KeybindNames.X360(code);
            }
            else if (settings is DS4BackPanelSettings ds4)
            {
                var code = button == DeckButton.L4 ? ds4.L4_DS4 : button == DeckButton.L5 ? ds4.L5_DS4 : button == DeckButton.R4 ? ds4.R4_DS4 : ds4.R5_DS4;
                if (code != VirtualDS4Code.None)
                    return KeybindNames.DS4(code);
            }

            return Nothing;
        }
    }
}
