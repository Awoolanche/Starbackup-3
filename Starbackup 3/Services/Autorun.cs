// Starbackup 3/Services/AutorunService.cs

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Starbackup_3.Services
{
    public class AutorunService
    {
        private readonly AutoExitService? _autoExitService;

        public AutorunService(AutoExitService? autoExitService = null)
        {
            _autoExitService = autoExitService;
        }
        public async Task StartGameAfterDelayAsync(string starboundExePath, bool shouldAutoExit)
        {
            if (string.IsNullOrEmpty(starboundExePath) || !File.Exists(starboundExePath))
            {
                Logger.Instance.Log($"Cannot autorun Starbound: Executable not found at '{starboundExePath}'.", LogLevel.Error);
                return;
            }

            try
            {
                Logger.Instance.Log("Backup complete. Starting Starbound in 5 seconds...", LogLevel.Info);
                await Task.Delay(5000);

                string? workingDirectory = Path.GetDirectoryName(Path.GetDirectoryName(starboundExePath));

                ProcessStartInfo startInfo = new ProcessStartInfo(starboundExePath)
                {
                    UseShellExecute = true,
                    WorkingDirectory = workingDirectory
                };

                Process.Start(startInfo);
                Logger.Instance.Log("Starbound started successfully.", LogLevel.Success);

                if (_autoExitService != null && shouldAutoExit)
                {
                    await _autoExitService.ExitAfterDelayAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Failed to start Starbound: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
