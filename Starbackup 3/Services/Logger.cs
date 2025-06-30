using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace Starbackup_3.Services;

public class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    public static Logger Instance => _instance.Value;

    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    private Logger() { }

    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var newEntry = new LogEntry(message, level);
            LogEntries.Add(newEntry);
            if (LogEntries.Count > 1000)
                LogEntries.RemoveAt(0);
        });
    }

    public void Clear()
    {
        Dispatcher.UIThread.Post(() => LogEntries.Clear());
    }
}