using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Starbackup_3.Services
{
    public class AppSettings
    {
        public string? StarboundPath { get; set; }
        public string? SteamWorkshopPath { get; set; }
        public bool AutorunEnabled { get; set; } = false;
        public bool AutoExitEnabled { get; set; } = false;
        public bool AutobackupEnabled { get; set; } = false;
    }

    public class SettingsService
    {
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            if (string.IsNullOrEmpty(_settingsFilePath))
            {
                Logger.Instance.Log("Cannot save settings: Settings file path is not set.", LogLevel.Error);
                return;
            }

            try
            {
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(settings, jsonOptions);
                await File.WriteAllTextAsync(_settingsFilePath, jsonString);
                Logger.Instance.Log($"Settings saved successfully to: {_settingsFilePath}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Error saving settings: {ex.Message}", LogLevel.Error);
            }
        }

        public async Task<AppSettings> LoadSettingsAsync()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string jsonString = await File.ReadAllTextAsync(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(jsonString);

                    if (settings != null)
                    {
                        Logger.Instance.Log($"Settings loaded from: {_settingsFilePath}", LogLevel.Info);
                        return settings;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log($"Error loading settings from {_settingsFilePath}: {ex.Message}", LogLevel.Error);
                }
            }
            else
            {
                Logger.Instance.Log($"Settings file not found at '{_settingsFilePath}'. A new one will be created when settings are saved.", LogLevel.Info);
            }

            return new AppSettings();
        }
    }
}
