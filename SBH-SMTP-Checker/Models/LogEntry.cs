using System.Windows.Media;

namespace SBH_SMTP_Checker.Models;

public enum LogLevel { Information, Success, Error }

public sealed record LogEntry(DateTime Timestamp, LogLevel Level, string Message)
{
    public Brush Color => Level switch
    {
        LogLevel.Success => Brushes.ForestGreen,
        LogLevel.Error => Brushes.Firebrick,
        _ => Brushes.Black
    };
}
