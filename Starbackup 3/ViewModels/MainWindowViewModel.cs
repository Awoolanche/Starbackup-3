using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Starbackup_3.Services;
using Starbackup_3.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Starbackup_3.ViewModels;

public class WorkshopModInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Unknown Title";

    [JsonPropertyName("workshopLink")]
    public string WorkshopLink { get; set; } = string.Empty;
}

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<LogEntry> LogEntries => Logger.Instance.LogEntries;

    // Properties for UI Binding
    [ObservableProperty]
    private string? _selectedFolderPath = "No folder selected yet.";

    [ObservableProperty]
    private string? _storageStatus = "Waiting for folder selection...";

    [ObservableProperty]
    private string? _platformStatus = "Waiting for folder selection...";

    [ObservableProperty]
    private string? _executableStatus = "Waiting for folder selection...";

    [ObservableProperty]
    private string? _modStatus = "Waiting for folder selection...";

    // Steam Workshop Properties
    [ObservableProperty]
    private string? _steamWorkshopStatus = "Waiting for initial check...";

    [ObservableProperty]
    private string? _steamWorkshopPath = "Not set.";

    [ObservableProperty]
    private string _backupStatus = "Ready to backup.";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackupCommand))]
    private bool _isPathValid;

    // Log Visibility and Expansion Control
    [ObservableProperty]
    private bool _isLogVisible;

    [ObservableProperty]
    private bool _isLogExpanded = false;

    // Feature Settings
    [ObservableProperty]
    private bool _isAutorunEnabled;

    [ObservableProperty]
    private bool _isAutoExitEnabled;

    [ObservableProperty]
    private bool _isAutobackupEnabled;

    private readonly SettingsService _settingsService;
    private readonly AutorunService _autorunService;
    private readonly AutoExitService _autoExitService;
    private readonly AutobackupService _autobackupService;
    private string? _platformSubfolder;

    public MainWindowViewModel()
    {
        _settingsService = new SettingsService();
        _autoExitService = new AutoExitService();
        _autorunService = new AutorunService(_autoExitService);
        _autobackupService = new AutobackupService(new RelayCommand(async () => await BackupAsync(), CanBackup));

        LogEntries.CollectionChanged += OnLogEntriesChanged;
        _isLogVisible = LogEntries.Any();
    }

    private void SaveCurrentSettings()
    {
        if (!string.IsNullOrEmpty(SelectedFolderPath) && IsPathValid)
        {
            _ = _settingsService.SaveSettingsAsync(new AppSettings
            {
                StarboundPath = SelectedFolderPath,
                SteamWorkshopPath = SteamWorkshopPath,
                AutorunEnabled = IsAutorunEnabled,
                AutoExitEnabled = IsAutoExitEnabled,
                AutobackupEnabled = IsAutobackupEnabled
            });
        }
    }

    partial void OnIsAutorunEnabledChanged(bool value)
    {
        Logger.Instance.Log($"Autorun after backup set to: {value}", LogLevel.Info);
        SaveCurrentSettings();
    }

    partial void OnIsAutoExitEnabledChanged(bool value)
    {
        Logger.Instance.Log($"Auto-exit after backup set to: {value}", LogLevel.Info);
        SaveCurrentSettings();
    }

    partial void OnIsAutobackupEnabledChanged(bool value)
    {
        Logger.Instance.Log($"Autobackup on startup set to: {value}", LogLevel.Info);
        SaveCurrentSettings();
    }

    public async Task InitializeAsync(Window mainWindow)
    {
        Logger.Instance.Log("Initializing application...", LogLevel.Info);

        AppSettings settings = await _settingsService.LoadSettingsAsync();
        IsAutorunEnabled = settings.AutorunEnabled;
        IsAutoExitEnabled = settings.AutoExitEnabled;
        IsAutobackupEnabled = settings.AutobackupEnabled;

        if (!string.IsNullOrEmpty(settings.StarboundPath))
        {
            Logger.Instance.Log($"Found saved Starbound path: {settings.StarboundPath}", LogLevel.Info);
            ValidateStarboundPath(settings.StarboundPath);

            if (IsPathValid)
            {
                if (!string.IsNullOrEmpty(settings.SteamWorkshopPath))
                {
                    CheckAndSetSteamWorkshopPath(settings.SteamWorkshopPath);
                }
                else
                {
                    CheckAndSetSteamWorkshopPath();
                }

                if (IsAutobackupEnabled)
                {
                    Logger.Instance.Log("Autobackup enabled and path is valid. Starting autobackup...", LogLevel.Info);
                    await _autobackupService.PerformAutobackupAsync();
                }
                return;
            }
            else
            {
                Logger.Instance.Log("Saved Starbound path is invalid.", LogLevel.Warning);
            }
        }

        Logger.Instance.Log("No valid Starbound path found in settings.", LogLevel.Info);
        await SelectFolderAsync(mainWindow);
        CheckAndSetSteamWorkshopPath();
    }

    [RelayCommand]
    private void ClearLog()
    {
        Logger.Instance.Clear();
    }

    [RelayCommand]
    private async Task OpenSettingsAsync(Window parentWindow)
    {
        Logger.Instance.Log("Opening settings window...", LogLevel.Info);
        var settingsWindow = new SettingsWindow
        {
            DataContext = this
        };
        await settingsWindow.ShowDialog(parentWindow);
    }

    [RelayCommand]
    private async Task SelectFolderAsync(Window parentWindow)
    {
        var topLevel = TopLevel.GetTopLevel(parentWindow);
        if (topLevel is null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select your Starbound root folder",
            AllowMultiple = false
        });

        if (folders.Any())
        {
            string selectedPath = folders[0].Path.LocalPath;
            ValidateStarboundPath(selectedPath);

            if (IsPathValid)
            {
                await _settingsService.SaveSettingsAsync(new AppSettings
                {
                    StarboundPath = SelectedFolderPath,
                    SteamWorkshopPath = SteamWorkshopPath,
                    AutorunEnabled = IsAutorunEnabled,
                    AutoExitEnabled = IsAutoExitEnabled,
                    AutobackupEnabled = IsAutobackupEnabled
                });
                Logger.Instance.Log("Starbound path saved to settings.", LogLevel.Info);
            }
        }
        else
        {
            Logger.Instance.Log("Folder selection cancelled.", LogLevel.Info);
        }
    }

    [RelayCommand]
    private async Task SelectSteamWorkshopFolderAsync(Window parentWindow)
    {
        var topLevel = TopLevel.GetTopLevel(parentWindow);
        if (topLevel is null) return;
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select your Steam Workshop Starbound content folder",
            AllowMultiple = false
        });

        if (folders.Any())
        {
            CheckAndSetSteamWorkshopPath(folders[0].Path.LocalPath);
            SaveCurrentSettings();
        }
        else
        {
            Logger.Instance.Log("Steam Workshop folder selection cancelled.", LogLevel.Info);
        }
    }

    [RelayCommand(CanExecute = nameof(CanBackup))]
    private async Task BackupAsync()
    {
        if (!CanBackup()) return;

        Logger.Instance.Log("Starting backup...", LogLevel.Info);
        BackupStatus = "Backup in progress, please wait...";

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string timestamp = DateTime.Now.ToString("ddMMyyyyHHmm");
        string archiveName = $"{timestamp}.zip";
        bool backupSucceeded = false;

        try
        {
            await Task.Run(() =>
            {
                string sourceStoragePath = Path.Combine(SelectedFolderPath!, "storage");
                string sourceModsPath = Path.Combine(SelectedFolderPath!, "mods");
                string backupDirectory = Path.Combine(SelectedFolderPath!, "backup");

                Directory.CreateDirectory(backupDirectory);
                string destinationArchivePath = Path.Combine(backupDirectory, archiveName);

                using var zipArchive = ZipFile.Open(destinationArchivePath, ZipArchiveMode.Create);

                void AddFolderToZip(string sourceDir, string zipRootFolder)
                {
                    if (!Directory.Exists(sourceDir))
                    {
                        Logger.Instance.Log($"Warning: Directory not found: {sourceDir}", LogLevel.Warning);
                        return;
                    }
                    var filesToZip = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
                    foreach (var file in filesToZip)
                    {
                        string relativePath = Path.GetRelativePath(sourceDir, file);
                        string entryName = Path.Combine(zipRootFolder, relativePath);
                        zipArchive.CreateEntryFromFile(file, entryName);
                    }
                }

                AddFolderToZip(sourceStoragePath, "storage");
                AddFolderToZip(sourceModsPath, "mods");

                if (Directory.Exists(SteamWorkshopPath))
                {
                    List<WorkshopModInfo> workshopMods = new();
                    foreach (string modIdFolder in Directory.EnumerateDirectories(SteamWorkshopPath))
                    {
                        string modId = new DirectoryInfo(modIdFolder).Name;
                        string modTitle = "Unknown Title";
                        try
                        {
                            var modinfoFiles = Directory.GetFiles(modIdFolder, "*.modinfo", SearchOption.AllDirectories);
                            if (modinfoFiles.Any())
                            {
                                string modinfoContent = File.ReadAllText(modinfoFiles.First());
                                using var doc = JsonDocument.Parse(modinfoContent);
                                if (doc.RootElement.TryGetProperty("name", out JsonElement nameElem))
                                    modTitle = nameElem.GetString() ?? modTitle;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Log($"Error reading .modinfo in {modIdFolder}: {ex.Message}", LogLevel.Error);
                        }
                        workshopMods.Add(new WorkshopModInfo
                        {
                            Id = modId,
                            Title = modTitle,
                            WorkshopLink = $"https://steamcommunity.com/sharedfiles/filedetails/?id={modId}"
                        });
                    }
                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    string workshopJson = JsonSerializer.Serialize(workshopMods, jsonOptions);
                    var workshopJsonEntry = zipArchive.CreateEntry("workshop.json");
                    using var writer = new StreamWriter(workshopJsonEntry.Open());
                    writer.Write(workshopJson);
                }
            });

            backupSucceeded = true;
            stopwatch.Stop();
            string elapsedTime = $"{stopwatch.Elapsed:hh\\:mm\\:ss\\.ff}";
            BackupStatus = $"✓ Backup successful! Saved to 'backup/{archiveName}'";
            Logger.Instance.Log($"Backup completed successfully in {elapsedTime}. Saved to 'backup/{archiveName}'", LogLevel.Success);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            string elapsedTime = $"{stopwatch.Elapsed:hh\\:mm\\:ss\\.ff}";
            BackupStatus = $"✗ Backup failed: {ex.Message}";
            Logger.Instance.Log($"Backup failed in {elapsedTime}: {ex.Message}", LogLevel.Error);
        }

        // --- AUTOMATION LOGIC ---
        if (backupSucceeded)
        {
            if (IsAutorunEnabled)
            {
                if (!string.IsNullOrEmpty(_platformSubfolder))
                {
                    string exePath = Path.Combine(SelectedFolderPath!, _platformSubfolder, "starbound.exe");
                    _ = _autorunService.StartGameAfterDelayAsync(exePath, IsAutoExitEnabled);
                }
                else
                {
                    Logger.Instance.Log("Could not autorun Starbound: platform subfolder not identified.", LogLevel.Warning);
                }
            }
            if (IsAutoExitEnabled && !IsAutorunEnabled)
            {
                _ = _autoExitService.ExitAfterDelayAsync();
            }
        }
    }

    private bool CanBackup() => IsPathValid;

    private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && !_isLogVisible)
        {
            IsLogVisible = true;
        }
    }

    public void CheckAndSetSteamWorkshopPath(string? path = null)
    {
        string workshopCandidatePath = path ?? @"C:\Program Files (x86)\Steam\steamapps\workshop\content\211820";

        if (Directory.Exists(workshopCandidatePath))
        {
            SteamWorkshopPath = workshopCandidatePath;
            SteamWorkshopStatus = "✓ Steam Workshop folder found.";
            Logger.Instance.Log($"Steam Workshop folder found: {SteamWorkshopPath}", LogLevel.Success);
        }
        else
        {
            SteamWorkshopPath = "Not set.";
            SteamWorkshopStatus = "✗ Steam Workshop folder NOT found.";
            Logger.Instance.Log("Steam Workshop folder NOT found. Please select it manually.", LogLevel.Warning);
        }
    }

    private void ValidateStarboundPath(string path)
    {
        bool isOverallValid = true;
        _platformSubfolder = null;

        SelectedFolderPath = path;
        Logger.Instance.Log($"Validating Starbound path: {path}", LogLevel.Info);

        if (Directory.Exists(Path.Combine(path, "storage")))
        {
            StorageStatus = "✓ 'storage' folder found.";
        }
        else
        {
            StorageStatus = "✗ 'storage' folder NOT found.";
            isOverallValid = false;
        }

        if (Directory.Exists(Path.Combine(path, "mods")))
        {
            ModStatus = "✓ 'mods' folder found.";
        }
        else
        {
            ModStatus = "✗ 'mods' folder NOT found.";
            isOverallValid = false;
        }

        if (Directory.Exists(Path.Combine(path, "win64")))
        {
            _platformSubfolder = "win64";
        }
        else if (Directory.Exists(Path.Combine(path, "win")))
        {
            _platformSubfolder = "win";
        }

        if (_platformSubfolder != null)
        {
            PlatformStatus = $"✓ '{_platformSubfolder}' folder found.";
            if (File.Exists(Path.Combine(path, _platformSubfolder, "starbound.exe")))
            {
                ExecutableStatus = "✓ 'starbound.exe' found.";
            }
            else
            {
                ExecutableStatus = "✗ 'starbound.exe' NOT found.";
                isOverallValid = false;
            }
        }
        else
        {
            PlatformStatus = "✗ 'win' or 'win64' folder NOT found.";
            isOverallValid = false;
            ExecutableStatus = "i Skipped: Cannot check without platform folder.";
        }

        IsPathValid = isOverallValid;
        BackupStatus = isOverallValid ? "Folder is valid. Ready to backup." : "Folder is invalid. Cannot backup.";
        if (!isOverallValid)
        {
            SelectedFolderPath = "The selected folder is not a valid Starbound directory.";
            Logger.Instance.Log("The selected folder is not a valid Starbound directory.", LogLevel.Error);
        }
        else
        {
            Logger.Instance.Log("Starbound directory validation successful.", LogLevel.Success);
        }
    }
}
