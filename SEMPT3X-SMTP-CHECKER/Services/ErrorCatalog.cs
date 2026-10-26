using SEMPT3X_SMTP_CHECKER.Models;

namespace SEMPT3X_SMTP_CHECKER.Services;

public static class ErrorCatalog
{
    public static IReadOnlyList<ErrorDefinition> All { get; } =
    [
        new("DNS-001", "Host not found", "SMTP hostname is incorrect or DNS is unavailable.", "Überprüfen Sie den Hostnamen, die DNS-Einstellungen und die Netzwerkverbindung.", "SMTP"),
        new("TCP-001", "Connection refused", "The host is reachable but nothing accepts the selected port.", "Überprüfen Sie den Port sowie die Firewall- und Listenerkonfiguration des Servers.", "SMTP"),
        new("TCP-002", "Connection timeout", "A firewall or routing rule may silently block the connection.", "Überprüfen Sie ausgehende Firewallregeln, Routing, VPN und Einschränkungen des Anbieters.", "SMTP"),
        new("SMTP-5.7.0", "Authentication required", "The server requires authentication or rejected the authentication method.", "Aktivieren Sie die Authentifizierung und prüfen Sie, ob das Konto SMTP AUTH unterstützt.", "SMTP"),
        new("SMTP-5.7.1", "Relay/access denied", "The account may not be allowed to send as the selected sender or recipient.", "Erteilen Sie die Berechtigung zum Senden als bzw. zur Weiterleitung und prüfen Sie die Serverrichtlinie.", "SMTP"),
        new("SMTP-5.7.3", "Authentication unsuccessful", "Username/password is wrong or basic authentication is disabled.", "Überprüfen Sie die Anmeldedaten und die vom Server unterstützte Authentifizierungsrichtlinie.", "SMTP"),
        new("SMTP-5.1.1", "Mailbox unavailable", "The recipient address does not exist or is unavailable.", "Korrigieren Sie die Empfängeradresse oder wenden Sie sich an den zuständigen Administrator.", "SMTP"),
        new("TLS-001", "TLS negotiation failed", "Protocol/certificate mismatch, unsupported TLS, or the port expects implicit TLS.", "Überprüfen Sie Zertifikatsvertrauen, Port und STARTTLS-Unterstützung. Diese Anwendung verwendet STARTTLS über SmtpClient.", "SMTP"),
        new("AADSTS700016", "Application not found", "Client ID is wrong or the app is registered in another tenant.", "Überprüfen Sie die Mandanten-ID und Client-ID in Microsoft Entra ID.", "Microsoft 365"),
        new("AADSTS7000215", "Invalid client secret", "The secret value is incorrect, expired, or its ID was entered instead.", "Erstellen Sie einen neuen Clientschlüssel und geben Sie dessen Wert ein, nicht die Schlüssel-ID.", "Microsoft 365"),
        new("AADSTS7000222", "Client secret expired", "All configured client secrets have expired.", "Erstellen Sie einen neuen Clientschlüssel und aktualisieren Sie die Anwendungskonfiguration.", "Microsoft 365"),
        new("Authorization_RequestDenied", "Insufficient privileges", "The app lacks Mail.Send application permission or admin consent.", "Erteilen Sie die Microsoft Graph-Anwendungsberechtigung Mail.Send und die Zustimmung eines Administrators.", "Microsoft 365"),
        new("ErrorAccessDenied", "Mailbox access denied", "Application access policy or mailbox permissions block this sender.", "Überprüfen Sie die Exchange-Anwendungszugriffsrichtlinie und die Postfachlizenzierung des Absenders.", "Microsoft 365"),
        new("ErrorInvalidUser", "Invalid sender", "The sender user principal name or mailbox cannot be found.", "Überprüfen Sie die Absenderadresse und ob ein Exchange Online-Postfach vorhanden ist.", "Microsoft 365"),
        new("HTTP-400", "Bad request", "Address, attachment, or message JSON is invalid or too large.", "Prüfen Sie die Protokolldetails und verringern Sie die Größe der Anhänge.", "Microsoft 365"),
        new("HTTP-401", "Unauthorized", "The token is invalid, expired, or issued for the wrong audience.", "Überprüfen Sie die Mandanten- und App-Einstellungen sowie den Graph-Bereich .default.", "Microsoft 365"),
        new("HTTP-403", "Forbidden", "The app is authenticated but lacks permission or is restricted by policy.", "Erteilen Sie Mail.Send einschließlich Administratorzustimmung und prüfen Sie die Anwendungszugriffsrichtlinie.", "Microsoft 365"),
        new("HTTP-429", "Too many requests", "Microsoft Graph throttled the application.", "Warten Sie das Retry-After-Intervall ab und versuchen Sie es erneut.", "Microsoft 365")
    ];
}
