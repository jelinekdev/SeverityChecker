namespace SeverityChecker.Core.Models;

public sealed record ScanReport(
    DateTime ScannedAt,
    int TotalPackagesScanned,
    IReadOnlyList<VulnerabilityResult> Vulnerabilities
)
{
    public bool HasVulnerabilities => Vulnerabilities.Count > 0;
};