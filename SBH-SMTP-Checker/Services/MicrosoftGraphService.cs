using System.Net.Http.Headers;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Text.Json;
using SBH_SMTP_Checker.ViewModels;

namespace SBH_SMTP_Checker.Services;

public sealed class MicrosoftGraphService(AppLogger logger)
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };

    public async Task SendAsync(MainViewModel vm, CancellationToken cancellationToken)
    {
        logger.Info("Starting Microsoft identity platform client-credentials authentication...");
        var tokenUri = $"https://login.microsoftonline.com/{Uri.EscapeDataString(vm.TenantId)}/oauth2/v2.0/token";
        using var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = vm.ClientId,
            ["client_secret"] = vm.ClientSecret,
            ["scope"] = "https://graph.microsoft.com/.default",
            ["grant_type"] = "client_credentials"
        });
        using var tokenResponse = await Http.PostAsync(tokenUri, tokenContent, cancellationToken);
        logger.Info($"Microsoft token endpoint returned HTTP {(int)tokenResponse.StatusCode} {tokenResponse.ReasonPhrase}.");
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode) throw CreateGraphException("Token acquisition failed", tokenJson);

        using var tokenDocument = JsonDocument.Parse(tokenJson);
        var token = tokenDocument.RootElement.GetProperty("access_token").GetString()
                    ?? throw new InvalidOperationException("The token response did not contain an access token.");
        logger.Success("Microsoft access token acquired successfully (token value is not logged).");

        var attachments = new List<object>();
        foreach (var path in vm.Attachments)
        {
            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            attachments.Add(new Dictionary<string, object?>
            {
                ["@odata.type"] = "#microsoft.graph.fileAttachment",
                ["name"] = Path.GetFileName(path),
                ["contentType"] = "application/octet-stream",
                ["contentBytes"] = Convert.ToBase64String(bytes)
            });
        }

        var payload = new
        {
            message = new
            {
                subject = vm.Subject ?? string.Empty,
                body = new { contentType = "Text", content = vm.MessageBody ?? string.Empty },
                toRecipients = new[] { new { emailAddress = new { address = vm.RecipientEmail } } },
                attachments
            }
        };
        var endpoint = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(vm.SenderEmail)}/sendMail";
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        logger.Info("Sending email through Microsoft Graph...");
        using var response = await Http.SendAsync(request, cancellationToken);
        logger.Info($"Microsoft Graph returned HTTP {(int)response.StatusCode} {response.ReasonPhrase}.");
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) throw CreateGraphException("Microsoft Graph sendMail failed", responseBody);
        logger.Success("Microsoft Graph accepted the email for delivery.");
    }

    private static Exception CreateGraphException(string prefix, string response)
    {
        try
        {
            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;
            if (root.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.Object)
                    return new InvalidOperationException($"{prefix}: {error.GetProperty("code").GetString()} - {error.GetProperty("message").GetString()}");
                return new InvalidOperationException($"{prefix}: {error.GetString()}");
            }
        }
        catch (JsonException) { }
        return new InvalidOperationException($"{prefix}: the service returned an unsuccessful response.");
    }
}
