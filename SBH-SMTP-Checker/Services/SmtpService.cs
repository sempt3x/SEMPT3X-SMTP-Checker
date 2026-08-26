using System.Net;
using System.Net.Mail;
using SBH_SMTP_Checker.ViewModels;

namespace SBH_SMTP_Checker.Services;

public sealed class SmtpService(AppLogger logger)
{
    private const int DefaultTimeoutMilliseconds = 30000;
    private const int Port465TimeoutMilliseconds = 5000;

    public async Task SendAsync(MainViewModel vm, CancellationToken cancellationToken)
    {
        logger.Info($"Connecting to SMTP server {vm.SmtpServer}:{vm.SmtpPort}...");
        if (vm.UseTls) logger.Info("Starting TLS/SSL (STARTTLS when supported by the server)...");
        logger.Info("Authenticating with the supplied SMTP account...");

        using var message = new MailMessage(vm.SenderEmail, vm.RecipientEmail)
        {
            Subject = vm.Subject ?? string.Empty,
            Body = vm.MessageBody ?? string.Empty
        };
        foreach (var file in vm.Attachments) message.Attachments.Add(new Attachment(file));

        using var client = new SmtpClient(vm.SmtpServer, vm.SmtpPort)
        {
            EnableSsl = vm.UseTls,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(vm.SmtpUsername, vm.SmtpPassword),
            Timeout = vm.SmtpPort == 465 ? Port465TimeoutMilliseconds : DefaultTimeoutMilliseconds,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        logger.Info("Sending test email...");

        if (vm.SmtpPort != 465)
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        else
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(Port465TimeoutMilliseconds);
            try
            {
                await client.SendMailAsync(message, timeout.Token);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"SMTP connection or response on port 465 timed out after {Port465TimeoutMilliseconds / 1000} seconds.", ex);
            }
        }

        logger.Success("SMTP server accepted the test email for delivery.");
    }
}
