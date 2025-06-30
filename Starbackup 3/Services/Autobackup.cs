using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace Starbackup_3.Services
{
    public class AutobackupService
    {
        private readonly IRelayCommand _backupCommand;

        public AutobackupService(IRelayCommand backupCommand)
        {
            _backupCommand = backupCommand ?? throw new ArgumentNullException(nameof(backupCommand));
        }

        // Checking if the backup is possible
        public async Task PerformAutobackupAsync()
        {
            Logger.Instance.Log("Attempting autobackup...", LogLevel.Info);
            try
            {
                if (_backupCommand.CanExecute(null))
                {
                    await Task.Run(() => _backupCommand.Execute(null));
                    Logger.Instance.Log("Autobackup initiated successfully.", LogLevel.Success);
                }
                else
                {
                    Logger.Instance.Log("Autobackup cannot be performed: Backup conditions not met (e.g., invalid path).", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Error during autobackup initiation: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
