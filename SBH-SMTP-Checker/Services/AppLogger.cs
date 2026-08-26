using System.Collections.ObjectModel;
using System.Windows;
using SBH_SMTP_Checker.Models;

namespace SBH_SMTP_Checker.Services;

public sealed class AppLogger
{
    public ObservableCollection<LogEntry> Entries { get; } = [];
    public void Info(string message) => Add(LogLevel.Information, message);
    public void Success(string message) => Add(LogLevel.Success, message);
    public void Error(string message) => Add(LogLevel.Error, message);
    private void Add(LogLevel level, string message)
    {
        void Append() => Entries.Add(new(DateTime.Now, level, message));
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess()) Append(); else dispatcher.Invoke(Append);
    }
}
