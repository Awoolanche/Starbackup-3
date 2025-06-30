using System;

namespace Starbackup_3.Services;

public enum LogLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class LogEntry
{
    public string Message { get; }
    public LogLevel Level { get; }
    public DateTime Timestamp { get; }

    public LogEntry(string message, LogLevel level)
    {
        Message = message;
        Level = level;
        Timestamp = DateTime.Now;
    }

    public string FormattedMessage => $"[{Timestamp:HH:mm:ss}] {Message}";
}