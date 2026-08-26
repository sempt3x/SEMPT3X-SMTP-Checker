using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SBH_SMTP_Checker.Models;

public enum TestState { Pending, Running, Success, Failed, NotApplicable }

public sealed class StatusItem : INotifyPropertyChanged
{
    private TestState _state;
    private string _detail = "Nicht geprüft";
    public required string Name { get; init; }
    public TestState State { get => _state; set { _state = value; OnChanged(); OnChanged(nameof(Color)); } }
    public string Detail { get => _detail; set { _detail = value; OnChanged(); } }
    public Brush Color => State switch
    {
        TestState.Success => Brushes.ForestGreen,
        TestState.Failed => Brushes.Firebrick,
        TestState.Running => Brushes.DarkOrange,
        _ => Brushes.Gray
    };
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
