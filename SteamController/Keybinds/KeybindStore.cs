using System.Text.Json;
using System.Text.Json.Serialization;
using static CommonHelpers.Log;

namespace SteamController.Keybinds
{
    /// Holds keybind profiles, persists them to JSON and tracks which one is active
    public static class KeybindStore
    {
        public const String FileName = "SteamController.keybinds.json";

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private static readonly object lockObj = new object();
        private static KeybindConfig config;
        private static String? foregroundProcess;

        /// Raised when profiles are saved or the active profile changes (might be called from any thread)
        public static event Action? Changed;

        public static String ConfigPath
        {
            get { return Path.Combine(AppContext.BaseDirectory, FileName); }
        }

        /// Profile currently applied to the controller (never modified, replaced on change)
        public static KeybindProfile Active { get; private set; }

        /// Process name that selected the active profile, or null if the default profile is used
        public static String? ActiveGame { get; private set; }

        static KeybindStore()
        {
            config = Load();
            Active = config.Get(config.DefaultProfile) ?? new KeybindProfile() { Name = KeybindConfig.DefaultProfileName };
        }

        /// Returns a copy of the configuration that can be freely edited
        public static KeybindConfig GetConfig()
        {
            lock (lockObj)
            {
                return config.Clone();
            }
        }

        public static bool Save(KeybindConfig newConfig)
        {
            var copy = newConfig.Clone();
            copy.Normalize();

            lock (lockObj)
            {
                config = copy;
                UpdateActive();
            }

            try
            {
                File.WriteAllText(ConfigPath, JsonSerializer.Serialize(copy, jsonOptions));
                return true;
            }
            catch (Exception e)
            {
                TraceException("KeybindStore", "Save", e);
                return false;
            }
            finally
            {
                OnChanged();
            }
        }

        /// Sets the profile used when no game specific profile matches
        public static void SelectDefault(String name)
        {
            var newConfig = GetConfig();
            if (newConfig.Get(name) is null || newConfig.DefaultProfile == name)
                return;
            newConfig.DefaultProfile = name;
            Save(newConfig);
        }

        public static void SelectNextDefault()
        {
            var newConfig = GetConfig();
            var index = newConfig.Profiles.FindIndex((p) => p.Name == newConfig.DefaultProfile);
            var next = newConfig.Profiles[(index + 1) % newConfig.Profiles.Count];
            SelectDefault(next.Name);
        }

        /// Called periodically with the name of the foreground process
        public static void SetForegroundProcess(String? processName)
        {
            bool changed;

            lock (lockObj)
            {
                if (foregroundProcess == processName)
                    return;
                foregroundProcess = processName;
                changed = UpdateActive();
            }

            if (changed)
                OnChanged();
        }

        private static bool UpdateActive()
        {
            KeybindProfile? profile = null;
            String? game = null;

            if (foregroundProcess is not null)
            {
                profile = config.Profiles.FirstOrDefault((p) => p.MatchesGame(foregroundProcess));
                if (profile is not null)
                    game = foregroundProcess;
            }

            profile = profile ?? config.Get(config.DefaultProfile) ?? config.Profiles.First();

            if (Active == profile && ActiveGame == game)
                return false;

            Active = profile;
            ActiveGame = game;
            TraceLine("KeybindStore: Active profile: {0} (game: {1})", profile.Name, game ?? "none");
            return true;
        }

        private static KeybindConfig Load()
        {
            KeybindConfig? loaded = null;

            try
            {
                if (File.Exists(ConfigPath))
                    loaded = JsonSerializer.Deserialize<KeybindConfig>(File.ReadAllText(ConfigPath), jsonOptions);
            }
            catch (Exception e)
            {
                TraceException("KeybindStore", "Load", e);
            }

            loaded = loaded ?? new KeybindConfig();
            loaded.Normalize();
            return loaded;
        }

        private static void OnChanged()
        {
            try { Changed?.Invoke(); }
            catch (Exception e) { TraceException("KeybindStore", "Changed", e); }
        }
    }
}
