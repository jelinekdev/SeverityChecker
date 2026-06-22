using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SeverityChecker.Core.Interfaces;
using SeverityChecker.Core.Models;
using SeverityChecker.Core.Options;

namespace SeverityChecker.Infrastructure.Mail;

public sealed class MailNotificationService : INotificationService
{
    private readonly MailOptions _mailOptions;
    private readonly ILogger<MailNotificationService> _logger;

    public MailNotificationService(
        IOptions<MailOptions> mailOptions,
        ILogger<MailNotificationService> logger)
    {
        _mailOptions = mailOptions.Value;
        _logger = logger;
    }

    public async Task SendReportAsync(ScanReport report, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (!report.HasVulnerabilities)
        {
            _logger.LogInformation("No vulnerabilities found, skipping email notification");
            return;
        }

        var message = BuildMailMessage(report);

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _mailOptions.Host,
            _mailOptions.Port,
            _mailOptions.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
            cancellationToken);

        await client.AuthenticateAsync(
            _mailOptions.Username,
            _mailOptions.Password,
            cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        _logger.LogInformation("Vulnerability report sent to {Count} recipients", _mailOptions.Recipients.Count);
    }

    private MimeMessage BuildMailMessage(ScanReport report)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_mailOptions.FromName, _mailOptions.FromAddress));

        foreach (var recipient in _mailOptions.Recipients)
            message.To.Add(MailboxAddress.Parse(recipient));

        message.Subject = $"[SeverityChecker] {report.Vulnerabilities.Count} Vulnerabilities Found – {report.ScannedAt:yyyy-MM-dd}";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = BuildHtmlBody(report)
        };

        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }

    private static string BuildHtmlBody(ScanReport report)
    {
        var rows = string.Join("\n", report.Vulnerabilities.Select(v => $"""
            <tr>
                <td style="padding:8px;border:1px solid #ddd;">{v.CveId}</td>
                <td style="padding:8px;border:1px solid #ddd;">{v.Package.Name}</td>
                <td style="padding:8px;border:1px solid #ddd;">{v.Package.Version}</td>
                <td style="padding:8px;border:1px solid #ddd;">
                    <span style="color:{GetSeverityColor(v.Severity)};font-weight:bold;">{v.Severity}</span>
                </td>
                <td style="padding:8px;border:1px solid #ddd;">{v.FixedVersion}</td>
                <td style="padding:8px;border:1px solid #ddd;">
                    <a href="{v.ReferenceUrl}">Details</a>
                </td>
            </tr>
        """));

        return $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"/></head>
            <body style="font-family:Arial,sans-serif;padding:20px;">
                <h2 style="color:#d9534f;">⚠️ SeverityChecker Report</h2>
                <p>Scan completed at: <strong>{report.ScannedAt:yyyy-MM-dd HH:mm:ss} UTC</strong></p>
                <p>Total packages scanned: <strong>{report.TotalPackagesScanned}</strong></p>
                <p>Vulnerabilities found: <strong style="color:#d9534f;">{report.Vulnerabilities.Count}</strong></p>

                <table style="border-collapse:collapse;width:100%;margin-top:20px;">
                    <thead>
                        <tr style="background:#f5f5f5;">
                            <th style="padding:8px;border:1px solid #ddd;text-align:left;">CVE ID</th>
                            <th style="padding:8px;border:1px solid #ddd;text-align:left;">Package</th>
                            <th style="padding:8px;border:1px solid #ddd;text-align:left;">Version</th>
                            <th style="padding:8px;border:1px solid #ddd;text-align:left;">Severity</th>
                            <th style="padding:8px;border:1px solid #ddd;text-align:left;">Fixed In</th>
                            <th style="padding:8px;border:1px solid #ddd;text-align:left;">Reference</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>

                <p style="margin-top:30px;color:#999;font-size:12px;">
                    This report was generated automatically by SeverityChecker.
                </p>
            </body>
            </html>
            """;
    }

    private static string GetSeverityColor(string severity) => severity switch
    {
        "CRITICAL" => "#7b0000",
        "HIGH"     => "#d9534f",
        "MEDIUM"   => "#f0ad4e",
        "LOW"      => "#5bc0de",
        _          => "#999999"
    };
}