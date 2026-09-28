namespace SeverityChecker.Models;

public static class SeverityParser
{
    public static Severity Parse(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "CRITICAL" => Severity.Critical,
        "HIGH" => Severity.High,
        "MODERATE" or "MEDIUM" => Severity.Moderate,
        "LOW" => Severity.Low,
        _ => Severity.Unknown
    };
}
