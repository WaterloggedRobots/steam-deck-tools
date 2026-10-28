using System.Text.Json.Serialization;
using SteamController.ProfilesSettings;

namespace SteamController.Keybinds
{
    /// Physical buttons of the Steam Deck that can be bound
    public enum DeckButton
    {
        A, B, X, Y,
        DPadUp, DPadDown, DPadLeft, DPadRight,
        L1, R1, L2, R2,
        L4, L5, R4, R5,
        LeftStick, RightStick,
        LeftPad, RightPad,
        Menu, Options,
        Steam, QuickAccess
    }

    /// When a binding is used: the button alone, or together with a held modifier
    public enum KeybindLayer
    {
        Pressed,
        Steam,
        QuickAccess
    }

    public enum KeybindActionType
    {
        Default,
        Disabled,
        Keyboard,
        Mouse,
        Xbox360,
        DS4,
        Special
    }

    public enum SpecialAction
    {
        None,
        OnScreenKeyboard,
        Screenshot,
        ToggleMagnifier,
        TaskManager,
        TaskView,
        ShowDesktop,
        ToggleFullscreen,
        CloseApp,
        GameBar,
        BrightnessUp,
        BrightnessDown,
        NextControllerProfile,
        BackToDesktopMode,
        NextKeybindProfile
    }

    public class KeybindAction
    {
        public KeybindActionType Type { get; set; } = KeybindActionType.Default;

        public VirtualKeyCode Key { get; set; } = VirtualKeyCode.None;
        public bool Ctrl { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }
        public bool Win { get; set; }

        public Devices.MouseController.Button Mouse { get; set; } = Devices.MouseController.Button.Left;
        public VirtualX360Code X360 { get; set; } = VirtualX360Code.None;
        public VirtualDS4Code DS4 { get; set; } = VirtualDS4Code.None;
        public SpecialAction Special { get; set; } = SpecialAction.None;

        public KeybindAction Clone()
        {
            return (KeybindAction)MemberwiseClone();
        }
    }

    public class KeybindBinding
    {
        public KeybindLayer Layer { get; set; }
        public DeckButton Button { get; set; }
        public KeybindAction Action { get; set; } = new KeybindAction();

        public KeybindBinding Clone()
        {
            return new KeybindBinding() { Layer = Layer, Button = Button, Action = Action.Clone() };
        }
    }

    public class KeybindProfile
    {
        public String Name { get; set; } = "";

        /// Process names (without .exe) that activate this profile when in foreground
        public List<String> Games { get; set; } = new List<String>();

        public List<KeybindBinding> Bindings { get; set; } = new List<KeybindBinding>();

        public KeybindAction? Find(KeybindLayer layer, DeckButton button)
        {
            return Bindings.FirstOrDefault((b) => b.Layer == layer && b.Button == button)?.Action;
        }

        public void Set(KeybindLayer layer, DeckButton button, KeybindAction? action)
        {
            Bindings.RemoveAll((b) => b.Layer == layer && b.Button == button);
            if (action is not null && action.Type != KeybindActionType.Default)
                Bindings.Add(new KeybindBinding() { Layer = layer, Button = button, Action = action.Clone() });
        }

        public bool MatchesGame(String processName)
        {
            return Games.Any((game) => String.Equals(NormalizeGame(game), processName, StringComparison.OrdinalIgnoreCase));
        }

        public static String NormalizeGame(String game)
        {
            game = game.Trim();
            if (game.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                game = game.Substring(0, game.Length - 4);
            return game;
        }

        public KeybindProfile Clone()
        {
            return new KeybindProfile()
            {
                Name = Name,
                Games = new List<String>(Games),
                Bindings = Bindings.Select((b) => b.Clone()).ToList()
            };
        }
    }

    public class KeybindConfig
    {
        public const String DefaultProfileName = "Default";

        public int Version { get; set; } = 1;

        /// Profile used when no game specific profile matches
        public String DefaultProfile { get; set; } = DefaultProfileName;

        public List<KeybindProfile> Profiles { get; set; } = new List<KeybindProfile>();

        public KeybindProfile? Get(String name)
        {
            return Profiles.FirstOrDefault((p) => p.Name == name);
        }

        /// Make sure that there is at least one profile and the default one exists
        public void Normalize()
        {
            Profiles.RemoveAll((p) => String.IsNullOrWhiteSpace(p.Name));
            if (Profiles.Count == 0)
                Profiles.Add(new KeybindProfile() { Name = DefaultProfileName });
            if (Get(DefaultProfile) is null)
                DefaultProfile = Profiles.First().Name;
        }

        public KeybindConfig Clone()
        {
            return new KeybindConfig()
            {
                Version = Version,
                DefaultProfile = DefaultProfile,
                Profiles = Profiles.Select((p) => p.Clone()).ToList()
            };
        }
    }
}
