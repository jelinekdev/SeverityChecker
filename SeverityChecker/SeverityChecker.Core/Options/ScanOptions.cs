namespace SeverityChecker.Core.Options;

public sealed class ScanOptions
{
    public const string SectionName = "Scan";

    public string ProjectsBasePath { get; init; } = string.Empty;
    public string CronExpression { get; init; } = "0 8 * * *";
    public IReadOnlyList<string> SeverityFilter { get; init; } = ["CRITICAL", "HIGH"];
}