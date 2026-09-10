using Shouldly;
using SeverityChecker.Worker.Configuration;
using SeverityChecker.Worker.Domain;
using Xunit;

namespace SeverityChecker.Worker.Tests.Configuration;

public class NotificationOptionsValidatorTests
{
    private readonly NotificationOptionsValidator _sut = new();

    [Fact]
    public void ShouldSucceedWhenDisabledRegardlessOfOtherValues()
    {
        var options = new NotificationOptions { Enabled = false };
        var result = _sut.Validate(name: null, options);
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void ShouldSucceedWhenEnabledWithCompleteConfiguration()
    {
        var options = new NotificationOptions
        {
            Enabled = true,
            MinimumSeverity = Severity.High,
            Recipient = "mixje97@gmail.com",
            Smtp = new SmtpOptions { Host = "smtp.gmail.com", Port = 587 }
        };
        var result = _sut.Validate(name: null, options);
        result.Succeeded.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void ShouldFailWhenEnabledWithInvalidRecipient(string recipient)
    {
        var options = new NotificationOptions
        {
            Enabled = true,
            Recipient = recipient,
            Smtp = new SmtpOptions { Host = "smtp.gmail.com" }
        };
        var result = _sut.Validate(name: null, options);
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain(f => f.Contains(nameof(NotificationOptions.Recipient)));
    }

    [Fact]
    public void ShouldFailWhenEnabledWithoutSmtpHost()
    {
        var options = new NotificationOptions
        {
            Enabled = true,
            Recipient = "mixje97@gmail.com",
            Smtp = new SmtpOptions { Host = "" }
        };
        var result = _sut.Validate(name: null, options);
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain(f => f.Contains(nameof(SmtpOptions.Host)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(70000)]
    public void ShouldFailWhenEnabledWithInvalidPort(int port)
    {
        var options = new NotificationOptions
        {
            Enabled = true,
            Recipient = "mixje97@gmail.com",
            Smtp = new SmtpOptions { Host = "smtp.gmail.com", Port = port }
        };
        var result = _sut.Validate(name: null, options);
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain(f => f.Contains(nameof(SmtpOptions.Port)));
    }
}
