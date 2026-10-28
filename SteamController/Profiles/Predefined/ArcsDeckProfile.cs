using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamController.Profiles.Predefined
{
    // Controls for the Arcs Sandbox Steam Deck edition (robot arm jogging).
    //
    // The Arcs app selects this profile through the shared DesiredProfile on
    // start and selects the previous profile again on exit. It is only
    // selectable while the app holds the "ArcsDeckActive" mutex, so if the app
    // dies without restoring, the controller falls back on its own.
    //
    // - Sticks: X360 sticks (read by the app over XInput)
    // - L4 / R4: X360 LB / RB, L5 / R5: X360 LT / RT (L1 / R1 send nothing)
    // - Right trackpad: mouse, R2: left button, L2 or right pad click: right button
    // - Left trackpad: scroll (zoom)
    public sealed class ArcsDeckProfile : X360HapticProfile
    {
        public const String ActiveMutexName = "ArcsDeckActive";

        private static readonly TimeSpan AppCheckInterval = TimeSpan.FromMilliseconds(500);
        private DateTime appCheckedAt = DateTime.MinValue;
        private bool appRunning;

        public ArcsDeckProfile()
        {
            Visible = false;
        }

        public override bool Selected(Context context)
        {
            return base.Selected(context) && AppRunning();
        }

        private bool AppRunning()
        {
            var now = DateTime.UtcNow;
            if (now - appCheckedAt < AppCheckInterval)
                return appRunning;
            appCheckedAt = now;

            try
            {
                if (Mutex.TryOpenExisting(ActiveMutexName, out var mutex))
                {
                    mutex.Dispose();
                    appRunning = true;
                }
                else
                {
                    appRunning = false;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Exists, but owned by another user: still running
                appRunning = true;
            }
            catch (Exception)
            {
                appRunning = false;
            }
            return appRunning;
        }

        public override Status Run(Context c)
        {
            if (base.Run(c).IsDone)
                return Status.Done;

            // Mouse buttons on the triggers, like the Desktop profile
            c.Mouse.Overwrite(Devices.MouseController.Button.Left, c.Steam.BtnR2);
            c.Mouse.Overwrite(Devices.MouseController.Button.Right, c.Steam.BtnL2 || c.Steam.BtnRPadPress);

            // Back buttons drive the arm: up / down and yaw
            c.X360.Overwrite(Xbox360Button.LeftShoulder, c.Steam.BtnL4);
            c.X360.Overwrite(Xbox360Button.RightShoulder, c.Steam.BtnR4);
            c.X360[Xbox360Slider.LeftTrigger] = c.Steam.BtnL5 ? short.MaxValue : (short)0;
            c.X360[Xbox360Slider.RightTrigger] = c.Steam.BtnR5 ? short.MaxValue : (short)0;

            return Status.Continue;
        }

        protected override void BackPanelShortcuts(Context c)
        {
            // L4, L5, R4, R5 are mapped in Run; skip the X360 / keyboard
            // mappings from Settings so they do not fire twice.
        }
    }
}
