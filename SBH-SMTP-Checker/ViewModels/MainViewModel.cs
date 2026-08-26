using System.Collections.ObjectModel;
using System.Net.Mail;
using System.IO;
using SBH_SMTP_Checker.Models;
using SBH_SMTP_Checker.Services;

namespace SBH_SMTP_Checker.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly AppLogger _logger = new();
    private readonly NetworkTestService _network;
    private readonly SmtpService _smtp;
    private readonly MicrosoftGraphService _graph;
    private CancellationTokenSource? _cancellation;
    private int _connectionTypeIndex;
    private bool _isBusy;
    private string _smtpServer = "smtp.example.com", _smtpUsername = "", _smtpPassword = "";
    private int _smtpPort = 587;
    private bool _useTls = true;
    private string _tenantId = "", _clientId = "", _clientSecret = "", _senderEmail = "", _recipientEmail = "", _subject = "SMTP-Konfigurationstest", _messageBody = "Dies ist eine Test-E-Mail.";

    public MainViewModel()
    {
        _network = new(_logger); _smtp = new(_logger); _graph = new(_logger);
        SendCommand = new AsyncRelayCommand(RunAsync, () => !IsBusy);
        CancelCommand = new RelayCommand(() => _cancellation?.Cancel(), () => IsBusy);
        ClearLogCommand = new RelayCommand(() => Logs.Clear());
        RemoveAttachmentCommand = new RelayCommand(RemoveSelectedAttachment);
        Statuses =
        [
            new() { Name = "Serverauflösung" }, new() { Name = "TCP-Port" },
            new() { Name = "Verbindung" }, new() { Name = "Authentifizierung" },
            new() { Name = "E-Mail-Übermittlung" }
        ];
        _logger.Info("Application ready. Select a connection type and enter the configuration.");
    }

    public string[] ConnectionTypes { get; } = ["Standard SMTP Server", "Microsoft 365 / Microsoft Online"];
    public int ConnectionTypeIndex { get => _connectionTypeIndex; set { if (Set(ref _connectionTypeIndex, value)) { OnChanged(nameof(IsStandardSmtp)); OnChanged(nameof(IsMicrosoft365)); ResetStatuses(); } } }
    public bool IsStandardSmtp => ConnectionTypeIndex == 0;
    public bool IsMicrosoft365 => !IsStandardSmtp;
    public string SmtpServer { get => _smtpServer; set => Set(ref _smtpServer, value); }
    public string SmtpUsername { get => _smtpUsername; set => Set(ref _smtpUsername, value); }
    public string SmtpPassword { get => _smtpPassword; set => Set(ref _smtpPassword, value); }
    public int SmtpPort { get => _smtpPort; set => Set(ref _smtpPort, value); }
    public bool UseTls { get => _useTls; set => Set(ref _useTls, value); }
    public string TenantId { get => _tenantId; set => Set(ref _tenantId, value); }
    public string ClientId { get => _clientId; set => Set(ref _clientId, value); }
    public string ClientSecret { get => _clientSecret; set => Set(ref _clientSecret, value); }
    public string SenderEmail { get => _senderEmail; set => Set(ref _senderEmail, value); }
    public string RecipientEmail { get => _recipientEmail; set => Set(ref _recipientEmail, value); }
    public string Subject { get => _subject; set => Set(ref _subject, value); }
    public string MessageBody { get => _messageBody; set => Set(ref _messageBody, value); }
    public bool IsBusy { get => _isBusy; private set { if (Set(ref _isBusy, value)) { SendCommand.Raise(); CancelCommand.RaiseCanExecuteChanged(); } } }
    public ObservableCollection<string> Attachments { get; } = [];
    public string? SelectedAttachment { get; set; }
    public ObservableCollection<LogEntry> Logs => _logger.Entries;
    public ObservableCollection<StatusItem> Statuses { get; }
    public AsyncRelayCommand SendCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand ClearLogCommand { get; }
    public RelayCommand RemoveAttachmentCommand { get; }

    public void AddAttachments(IEnumerable<string> paths)
    {
        foreach (var path in paths.Where(File.Exists).Where(path => !Attachments.Contains(path))) Attachments.Add(path);
        _logger.Info($"Attachment selection contains {Attachments.Count} file(s). File contents are not logged.");
    }

    private void RemoveSelectedAttachment()
    {
        if (SelectedAttachment is not null) Attachments.Remove(SelectedAttachment);
    }

    private async Task RunAsync()
    {
        ResetStatuses();
        try { Validate(); }
        catch (Exception ex) { _logger.Error($"Validation failed: {ex.Message}"); Statuses[4].State = TestState.Failed; Statuses[4].Detail = "Invalid input"; return; }

        IsBusy = true;
        _cancellation = new CancellationTokenSource();
        var cancellationToken = _cancellation.Token;
        _logger.Info($"Starting {(IsStandardSmtp ? "SMTP" : "Microsoft 365")} connection test.");
        try
        {
            if (IsStandardSmtp)
            {
                SetRunning(0, "Host wird aufgelöst");
                var addresses = await _network.ResolveAsync(SmtpServer, cancellationToken);
                SetSuccess(0, $"{addresses.Length} Adresse(n)");
                SetRunning(1, $"Port {SmtpPort} wird geprüft");
                await _network.TestPortAsync(SmtpServer, SmtpPort, cancellationToken);
                SetSuccess(1, $"Port {SmtpPort} offen");
                SetRunning(2, "Verbindung wird hergestellt"); SetRunning(3, "Authentifizierung läuft"); SetRunning(4, "E-Mail wird gesendet");
                await _smtp.SendAsync(this, cancellationToken);
                SetSuccess(2, UseTls ? "TLS-Verbindung hergestellt" : "Verbindung hergestellt");
                SetSuccess(3, "Anmeldedaten akzeptiert"); SetSuccess(4, "Vom SMTP-Server angenommen");
            }
            else
            {
                Statuses[0].State = Statuses[1].State = TestState.NotApplicable;
                Statuses[0].Detail = Statuses[1].Detail = "Wird über HTTPS verwaltet";
                SetRunning(2, "Verbindung mit Microsoft-Diensten wird hergestellt"); SetRunning(3, "Token wird abgerufen"); SetRunning(4, "Graph sendMail wird aufgerufen");
                await _graph.SendAsync(this, cancellationToken);
                SetSuccess(2, "HTTPS-Anfrage erfolgreich"); SetSuccess(3, "Token abgerufen"); SetSuccess(4, "Anfrage von Graph angenommen (202)");
            }
        }
        catch (TimeoutException ex)
        {
            _logger.Error($"Timeout: {Sanitize(ex.Message)}");
            FailRunning("Zeitüberschreitung — siehe Protokoll");
        }
        catch (OperationCanceledException) { _logger.Error("Operation cancelled by the user."); FailRunning("Abgebrochen"); }
        catch (Exception ex)
        {
            _logger.Error($"Operation failed: {ex.GetType().Name}: {Sanitize(ex.Message)}");
            FailRunning("Fehlgeschlagen — siehe Protokoll");
        }
        finally { _cancellation.Dispose(); _cancellation = null; IsBusy = false; }
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(SenderEmail) || string.IsNullOrWhiteSpace(RecipientEmail)) throw new ArgumentException("Sender and recipient email addresses are required.");
        _ = new MailAddress(SenderEmail); _ = new MailAddress(RecipientEmail);
        foreach (var path in Attachments) if (!File.Exists(path)) throw new FileNotFoundException($"Attachment not found: {Path.GetFileName(path)}");
        if (IsStandardSmtp)
        {
            if (string.IsNullOrWhiteSpace(SmtpServer)) throw new ArgumentException("SMTP server is required.");
            if (SmtpPort is < 1 or > 65535) throw new ArgumentOutOfRangeException(nameof(SmtpPort), "SMTP port must be between 1 and 65535.");
            if (string.IsNullOrWhiteSpace(SmtpUsername)) throw new ArgumentException("SMTP username is required.");
        }
        else if (string.IsNullOrWhiteSpace(TenantId) || string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret))
            throw new ArgumentException("Tenant ID, Client ID, and Client Secret are required.");
    }

    private static string Sanitize(string message) => message.Replace("\r", " ").Replace("\n", " ");
    private void ResetStatuses() { foreach (var item in Statuses) { item.State = TestState.Pending; item.Detail = "Nicht geprüft"; } }
    private void SetRunning(int index, string detail) { Statuses[index].State = TestState.Running; Statuses[index].Detail = detail; }
    private void SetSuccess(int index, string detail) { Statuses[index].State = TestState.Success; Statuses[index].Detail = detail; }
    private void FailRunning(string detail) { foreach (var item in Statuses.Where(x => x.State == TestState.Running)) { item.State = TestState.Failed; item.Detail = detail; } }
}
