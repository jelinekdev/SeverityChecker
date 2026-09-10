using SeverityChecker.Worker.Domain;

namespace SeverityChecker.Worker.Configuration;

public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";

    public bool Enabled { get; init; }

    public Severity MinimumSeverity { get; init; } = Severity.High;

    public string Recipient { get; init; } = string.Empty;

    public SmtpOptions Smtp { get; init; } = new();
}

public sealed class SmtpOptions
{
    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 587;

    public bool UseSsl { get; init; } = true;

    public string? Username { get; init; }

    public string? Password { get; init; }
}
