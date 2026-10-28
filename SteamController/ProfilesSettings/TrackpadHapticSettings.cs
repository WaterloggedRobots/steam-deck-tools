using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    [Category("3. Haptic")]
    internal sealed class TrackpadHapticSettings : CommonHelpers.BaseSettings
    {
        public const short MinTickDistance = 500;
        public const short MaxTickDistance = 20000;

        public static TrackpadHapticSettings Default { get; } = new TrackpadHapticSettings();

        public TrackpadHapticSettings() : base("TrackpadHapticSettings")
        {
        }

        [Description("Send a haptic tick when moving a finger across the trackpads, like on SteamOS")]
        public bool Enabled
        {
            get { return Get<bool>("Enabled", true); }
            set { Set("Enabled", value); }
        }

        [Description("Send a stronger haptic click when a trackpad is pressed down")]
        public bool ClickOnPress
        {
            get { return Get<bool>("ClickOnPress", true); }
            set { Set("ClickOnPress", value); }
        }

        public Devices.SteamController.HapticStyle HapticStyle
        {
            get { return Get<Devices.SteamController.HapticStyle>("HapticStyle", Devices.SteamController.HapticStyle.Weak); }
            set { Set("HapticStyle", value); }
        }

        [Description("Haptic intensity of a tick between -2dB and 10dB")]
        public sbyte Intensity
        {
            get { return Get<sbyte>("Intensity", 0); }
            set { Set("Intensity", Math.Clamp(value, HapticSettings.MinIntensity, HapticSettings.MaxIntensity)); }
        }

        [Description("Distance the finger has to travel between ticks, between 500 and 20000 (a trackpad is about 65000 wide)")]
        public short TickDistance
        {
            get { return Get<short>("TickDistance", 4000); }
            set { Set("TickDistance", Math.Clamp(value, MinTickDistance, MaxTickDistance)); }
        }
    }
}
