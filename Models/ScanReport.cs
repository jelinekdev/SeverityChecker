namespace SeverityChecker.Models;

public sealed record ScanReport(
    DateTimeOffset ScannedAtUtc,
    int ScannedDependencyCount,
    IReadOnlyList<Finding> Findings);
