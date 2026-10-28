using System.Diagnostics;
using ExternalHelpers;
using PowerControl.Helpers;
using SteamController.Profiles.Default;
using SteamController.ProfilesSettings;

namespace SteamController.Keybinds
{
    /// Applies the active keybind profile before the controller profile runs.
    ///
    /// A bound button is consumed for the whole press, so the built-in
    /// mapping of the controller profile does not see it. Which binding is
    /// used is decided when the button goes down: alone, with STEAM held
    /// or with the 3 dots (Quick Access) held.
    public static class KeybindRunner
    {
        public const String Consumed = "Keybinds";

        public static readonly DeckButton[] AllButtons = Enum.GetValues<DeckButton>();

        private static readonly Dictionary<DeckButton, KeybindAction> pressedActions = new Dictionary<DeckButton, KeybindAction>();

        /// Buttons that drive profile switching and shortcuts, so they cannot be remapped alone
        public static bool CanBind(KeybindLayer layer, DeckButton button)
        {
            switch (button)
            {
                case DeckButton.Steam:
                case DeckButton.QuickAccess:
                    return false;

                case DeckButton.Options:
                    return layer != KeybindLayer.Pressed;

                default:
                    return true;
            }
        }

        public static Devices.SteamButton GetButton(Devices.SteamController steam, DeckButton button)
        {
            switch (button)
            {
                case DeckButton.A: return steam.BtnA;
                case DeckButton.B: return steam.BtnB;
                case DeckButton.X: return steam.BtnX;
                case DeckButton.Y: return steam.BtnY;
                case DeckButton.DPadUp: return steam.BtnDpadUp;
                case DeckButton.DPadDown: return steam.BtnDpadDown;
                case DeckButton.DPadLeft: return steam.BtnDpadLeft;
                case DeckButton.DPadRight: return steam.BtnDpadRight;
                case DeckButton.L1: return steam.BtnL1;
                case DeckButton.R1: return steam.BtnR1;
                case DeckButton.L2: return steam.BtnL2;
                case DeckButton.R2: return steam.BtnR2;
                case DeckButton.L4: return steam.BtnL4;
                case DeckButton.L5: return steam.BtnL5;
                case DeckButton.R4: return steam.BtnR4;
                case DeckButton.R5: return steam.BtnR5;
                case DeckButton.LeftStick: return steam.BtnLeftStickPress;
                case DeckButton.RightStick: return steam.BtnRightStickPress;
                case DeckButton.LeftPad: return steam.BtnLPadPress;
                case DeckButton.RightPad: return steam.BtnRPadPress;
                case DeckButton.Menu: return steam.BtnMenu;
                case DeckButton.Options: return steam.BtnOptions;
                case DeckButton.Steam: return steam.BtnSteam;
                case DeckButton.QuickAccess: return steam.BtnQuickAccess;
                default: throw new ArgumentOutOfRangeException(nameof(button));
            }
        }

        /// Analog triggers are passed to X360/DS4 directly, so profiles ask if they should hold them back
        public static bool IsHeld(DeckButton button)
        {
            lock (pressedActions)
            {
                return pressedActions.ContainsKey(button);
            }
        }

        public static void Run(Context c, Profiles.Profile profile)
        {
            if (profile is not GuideShortcutsProfile shortcutsProfile)
            {
                lock (pressedActions)
                    pressedActions.Clear();
                return;
            }

            // Remapping buttons alone is only done when the controller is ours (not Steam Input)
            bool allowRemap = profile is BackPanelShortcutsProfile;
            var keybinds = KeybindStore.Active;

            bool steamHeld = c.Steam.BtnSteam.Hold(shortcutsProfile.HoldForShorcuts, ShortcutsProfile.ShortcutConsumed);
            bool quickAccessHeld = !steamHeld && c.Steam.BtnQuickAccess.Hold(shortcutsProfile.HoldForShorcuts, ShortcutsProfile.ShortcutConsumed);

            var layer = KeybindLayer.Pressed;
            if (steamHeld)
                layer = KeybindLayer.Steam;
            else if (quickAccessHeld)
                layer = KeybindLayer.QuickAccess;

            lock (pressedActions)
            {
                foreach (var deckButton in AllButtons)
                {
                    var button = GetButton(c.Steam, deckButton);

                    if (button.JustPressed() && CanBind(layer, deckButton) && (layer != KeybindLayer.Pressed || allowRemap))
                    {
                        var action = keybinds.Find(layer, deckButton);
                        if (action is not null && action.Type != KeybindActionType.Default && button.Hold(Consumed))
                        {
                            pressedActions[deckButton] = action;
                            if (action.Type == KeybindActionType.Special)
                                RunSpecial(c, action.Special);
                        }
                    }

                    if (!pressedActions.TryGetValue(deckButton, out var pressedAction))
                        continue;

                    if (button.Hold(Consumed))
                        ApplyHeld(c, profile, pressedAction);
                    else if (!button.Value)
                        pressedActions.Remove(deckButton);
                }
            }
        }

        private static void ApplyHeld(Context c, Profiles.Profile profile, KeybindAction action)
        {
            switch (action.Type)
            {
                case KeybindActionType.Keyboard:
                    // Modifiers go first, so that they are down before the key
                    if (action.Ctrl)
                        c.Keyboard[WindowsInput.VirtualKeyCode.LCONTROL] = true;
                    if (action.Shift)
                        c.Keyboard[WindowsInput.VirtualKeyCode.LSHIFT] = true;
                    if (action.Alt)
                        c.Keyboard[WindowsInput.VirtualKeyCode.LMENU] = true;
                    if (action.Win)
                        c.Keyboard[WindowsInput.VirtualKeyCode.LWIN] = true;
                    c.Keyboard[action.Key.ToWindowsInput()] = true;
                    break;

                case KeybindActionType.Mouse:
                    c.Mouse[action.Mouse] = true;
                    break;

                case KeybindActionType.Xbox360:
                    if (profile is Profiles.Predefined.X360Profile)
                        c.X360[action.X360.ToViGEm()] = true;
                    break;

                case KeybindActionType.DS4:
                    var ds4Button = action.DS4.ToDS4Button();
                    if (profile is Profiles.Predefined.DS4Profile && ds4Button is not null)
                        c.DS4[ds4Button.Value] = true;
                    break;
            }
        }

        private static void RunSpecial(Context c, SpecialAction special)
        {
            switch (special)
            {
                case SpecialAction.OnScreenKeyboard:
                    if (!OnScreenKeyboard.Toggle())
                        c.Keyboard.KeyPress(new WindowsInput.VirtualKeyCode[] { WindowsInput.VirtualKeyCode.LCONTROL, WindowsInput.VirtualKeyCode.LWIN }, WindowsInput.VirtualKeyCode.VK_O);
                    break;

                case SpecialAction.Screenshot:
                    c.Keyboard.KeyPress(WindowsInput.VirtualKeyCode.LWIN, WindowsInput.VirtualKeyCode.SNAPSHOT);
                    break;

                case SpecialAction.ToggleMagnifier:
                    if (Process.GetProcessesByName("Magnify").Any())
                        c.Keyboard.KeyPress(WindowsInput.VirtualKeyCode.LWIN, WindowsInput.VirtualKeyCode.ESCAPE);
                    else
                        c.Keyboard.KeyPress(WindowsInput.VirtualKeyCode.LWIN, WindowsInput.VirtualKeyCode.OEM_PLUS);
                    break;

                case SpecialAction.TaskManager:
                    c.Keyboard.KeyPress(new WindowsInput.VirtualKeyCode[] { WindowsInput.VirtualKeyCode.LCONTROL, WindowsInput.VirtualKeyCode.SHIFT }, WindowsInput.VirtualKeyCode.ESCAPE);
                    break;

                case SpecialAction.TaskView:
                    c.Keyboard.KeyPress(WindowsInput.VirtualKeyCode.LWIN, WindowsInput.VirtualKeyCode.TAB);
                    break;

                case SpecialAction.ShowDesktop:
                    c.Keyboard.KeyPress(WindowsInput.VirtualKeyCode.LWIN, WindowsInput.VirtualKeyCode.VK_D);
                    break;

                case SpecialAction.ToggleFullscreen:
                    c.Keyboard.KeyPress(WindowsInput.VirtualKeyCode.F11);
                    break;

                case SpecialAction.CloseApp:
                    Helpers.ForegroundProcess.Store();
                    c.Keyboard.KeyPress(WindowsInput.VirtualKeyCode.LMENU, WindowsInput.VirtualKeyCode.F4);
                    break;

                case SpecialAction.GameBar:
                    c.Keyboard.KeyPress(WindowsInput.VirtualKeyCode.LWIN, WindowsInput.VirtualKeyCode.VK_G);
                    break;

                case SpecialAction.BrightnessUp:
                    WindowsSettingsBrightnessController.Increase(5);
                    break;

                case SpecialAction.BrightnessDown:
                    WindowsSettingsBrightnessController.Increase(-5);
                    break;

                case SpecialAction.NextControllerProfile:
                    if (!c.SelectNext())
                        c.BackToDefault();
                    break;

                case SpecialAction.BackToDesktopMode:
                    c.BackToDefault();
                    break;

                case SpecialAction.NextKeybindProfile:
                    KeybindStore.SelectNextDefault();
                    c.X360.Beep();
                    break;
            }
        }
    }
}
