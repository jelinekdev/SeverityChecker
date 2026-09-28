using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SeverityChecker.Abstractions;
using SeverityChecker.Configuration;
using SeverityChecker.Models;

namespace SeverityChecker.Services;

public sealed class EmailNotificationSender(
    HttpClient httpClient,
    IOptions<EmailOptions> options,
    ILogger<EmailNotificationSender> logger) : INotificationSender
{
    private const string EndpointBaseUrl = "https://formsubmit.co/ajax/";
    private const string RefererUrl = "https://localhost/";

    public async Task SendAsync(ScanReport report, CancellationToken cancellationToken)
    {
        var settings = options.Value;

        if (!settings.Enabled)
        {
            logger.LogInformation("E-mail notifications are disabled. Report is not sent.");
            return;
        }
        if (settings.Recipients.Count == 0)
        {
            throw new InvalidOperationException("Email:Recipients is empty. Add at least one address.");
        }
        var recipient = settings.Recipients[0].Trim();
        using var request = new HttpRequestMessage(HttpMethod.Post, EndpointBaseUrl + recipient)
        {
            Content = JsonContent.Create(BuildPayload(report, settings))
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Referrer = new Uri(RefererUrl);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode || !IsSuccessResponse(responseBody))
        {
            throw new InvalidOperationException(
                $"FormSubmit did not accept the message. If this is the first run, an activation " +
                $"e-mail was sent to {recipient}: click the link in it (check spam) and restart. " +
                $"Response: {responseBody}");
        }
        logger.LogInformation("Report with {Count} findings sent to {Recipient}.", report.Findings.Count, recipient);
    }

    private static Dictionary<string, string> BuildPayload(ScanReport report, EmailOptions settings)
    {
        var payload = new Dictionary<string, string>
        {
            ["name"] = "SeverityChecker",
            ["message"] = ReportFormatter.ToPlainText(report),
            ["_subject"] = ReportFormatter.BuildSubject(report),
            ["_captcha"] = "false"
        };
        if (settings.Recipients.Count > 1)
        {
            payload["_cc"] = string.Join(",", settings.Recipients.Skip(1).Select(address => address.Trim()));
        }
        return payload;
    }

    private static bool IsSuccessResponse(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (!document.RootElement.TryGetProperty("success", out var success))
            {
                return false;
            }
            return success.ValueKind == JsonValueKind.True
                   || (success.ValueKind == JsonValueKind.String
                       && bool.TryParse(success.GetString(), out var parsed)
                       && parsed);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
