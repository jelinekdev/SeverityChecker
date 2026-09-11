namespace SeverityChecker.Worker.Configuration;

public sealed class NpmScannerOptions
{
    public const string SectionName = "Scanner:Npm";

    public bool IncludeDevDependencies { get; init; } = true;
}
