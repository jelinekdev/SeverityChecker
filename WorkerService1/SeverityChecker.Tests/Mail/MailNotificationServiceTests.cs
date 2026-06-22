using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using SeverityChecker.Core.Models;
using SeverityChecker.Core.Options;
using SeverityChecker.Infrastructure.Mail;

namespace SeverityChecker.Tests.Mail;

[TestFixture]
public sealed class MailNotificationServiceTests
{
    private MailNotificationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = Options.Create(new MailOptions
        {
            Host = "smtp.example.com",
            Port = 587,
            Username = "user@example.com",
            Password = "password",
            UseSsl = true,
            FromAddress = "checker@example.com",
            FromName = "SeverityChecker",
            Recipients = ["dev@example.com"]
        });

        _sut = new MailNotificationService(
            options,
            NullLogger<MailNotificationService>.Instance
        );
    }

    [Test]
    public async Task SendReportAsync_NoVulnerabilities_SkipsSending()
    {
        var report = new ScanReport(
            ScannedAt: DateTime.UtcNow,
            TotalPackagesScanned: 5,
            Vulnerabilities: []
        );

        Assert.DoesNotThrowAsync(() => _sut.SendReportAsync(report));
        await Task.CompletedTask;
    }

    [Test]
    public void SendReportAsync_NullReport_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.SendReportAsync(null!));
    }
}