using SteamController.Devices;
using SteamController.ProfilesSettings;
using HapticPad = SteamController.Devices.SteamController.HapticPad;
using HapticStyle = SteamController.Devices.SteamController.HapticStyle;

namespace SteamController.Helpers
{
    /// Recreates the SteamOS trackpad haptics: a tick every time the finger
    /// travels a set distance across a pad, and a click when the pad is pressed.
    public class TrackpadHaptics
    {
        private class PadState
        {
            public bool Touched;
            public int AnchorX, AnchorY;
        }

        private readonly PadState left = new PadState();
        private readonly PadState right = new PadState();

        private TrackpadHapticSettings Settings
        {
            get { return TrackpadHapticSettings.Default; }
        }

        public void Run(Context c)
        {
            if (!Settings.Enabled)
                return;

            RunPad(c, left, HapticPad.Left, c.Steam.BtnLPadTouch, c.Steam.BtnLPadPress, c.Steam.LPadX, c.Steam.LPadY);
            RunPad(c, right, HapticPad.Right, c.Steam.BtnRPadTouch, c.Steam.BtnRPadPress, c.Steam.RPadX, c.Steam.RPadY);
        }

        private void RunPad(Context c, PadState pad, HapticPad position, SteamButton touch, SteamButton press, SteamAxis x, SteamAxis y)
        {
            if (!touch.Value)
            {
                pad.Touched = false;
                return;
            }

            if (!pad.Touched)
            {
                // Finger just landed, start measuring travel from here
                pad.Touched = true;
                pad.AnchorX = x.Value;
                pad.AnchorY = y.Value;
            }

            if (press.JustPressed())
            {
                if (Settings.ClickOnPress)
                    c.Steam.SendHaptic(position, HapticStyle.Strong, Settings.Intensity);
                return;
            }

            // Measure straight line distance from the last tick rather than summing
            // every step, so jitter of a resting finger never adds up to a tick
            long dx = x.Value - pad.AnchorX;
            long dy = y.Value - pad.AnchorY;
            long distance = Settings.TickDistance;
            if (dx * dx + dy * dy < distance * distance)
                return;

            // SendHaptic returns false while the previous haptic is still being sent,
            // in that case keep the anchor so the tick fires on the next update
            if (c.Steam.SendHaptic(position, Settings.HapticStyle, Settings.Intensity))
            {
                pad.AnchorX = x.Value;
                pad.AnchorY = y.Value;
            }
        }
    }
}
