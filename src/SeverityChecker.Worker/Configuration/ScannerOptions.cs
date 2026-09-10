namespace SeverityChecker.Worker.Configuration;

public sealed class ScannerOptions
{
    public const string SectionName = "Scanner";

    public int ScanIntervalMinutes { get; init; } = 60;

    public IReadOnlyList<string> ScanPaths { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> ExcludedDirectories { get; init; } = new[]
    {
        ".git",
        "node_modules",
        "bin",
        "obj",
        "target",
        "build",
        "dist",
        ".venv"
    };
}
