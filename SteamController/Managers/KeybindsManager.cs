using SteamController.Keybinds;

namespace SteamController.Managers
{
    /// Activates game specific keybind profiles based on the foreground process
    public sealed class KeybindsManager : Manager
    {
        public override void Tick(Context context)
        {
            String? processName = null;

            using (var process = Helpers.ForegroundProcess.Find())
            {
                try { processName = process?.ProcessName; }
                catch (InvalidOperationException) { }
            }

            KeybindStore.SetForegroundProcess(processName);
        }
    }
}
