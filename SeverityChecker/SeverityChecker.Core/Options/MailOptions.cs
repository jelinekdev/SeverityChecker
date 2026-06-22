namespace SeverityChecker.Core.Options;

public sealed class MailOptions
{
    public const string SectionName = "Mail";

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool UseSsl { get; init; } = true;
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "SeverityChecker";
    public IReadOnlyList<string> Recipients { get; init; } = [];
}