using System;
using System.Threading.Tasks;

namespace Starbackup_3.Services
{
    public class AutoExitService
    {
        public async Task ExitAfterDelayAsync()
        {
            try
            {
                Logger.Instance.Log("Task complete. Application will close in 5 seconds...", LogLevel.Info);
                await Task.Delay(5000);

                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Failed to auto-exit application: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
