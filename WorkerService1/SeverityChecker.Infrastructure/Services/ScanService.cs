using Microsoft.Extensions.Logging;
using SeverityChecker.Core.Interfaces;
using SeverityChecker.Core.Models;
using SeverityChecker.Core.Options;
using SeverityChecker.Infrastructure.Parsers;
using Microsoft.Extensions.Options;

namespace SeverityChecker.Infrastructure.Services;

public sealed class ScanService : IScanService
{
    private readonly NuGetPackageParser _packageParser;
    private readonly IVulnerabilitySource _vulnerabilitySource;
    private readonly ILogger<ScanService> _logger;
    private readonly ScanOptions _scanOptions;

    public ScanService(
        NuGetPackageParser packageParser,
        IVulnerabilitySource vulnerabilitySource,
        ILogger<ScanService> logger,
        IOptions<ScanOptions> scanOptions)
    {
        _packageParser = packageParser;
        _vulnerabilitySource = vulnerabilitySource;
        _logger = logger;
        _scanOptions = scanOptions.Value;
    }

    public async Task<ScanReport> ScanAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting vulnerability scan at {BasePath}", _scanOptions.ProjectsBasePath);

        var packages = _packageParser.ParseDirectory(_scanOptions.ProjectsBasePath);

        _logger.LogInformation("Found {Count} unique packages to scan", packages.Count);

        var vulnerabilities = new List<VulnerabilityResult>();

        foreach (var package in packages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var results = await _vulnerabilitySource.GetVulnerabilitiesAsync(package, cancellationToken);
            vulnerabilities.AddRange(results);

            if (results.Count > 0)
                _logger.LogWarning("Found {Count} vulnerabilities in {Package} {Version}",
                    results.Count, package.Name, package.Version);
        }

        var report = new ScanReport(
            ScannedAt: DateTime.UtcNow,
            TotalPackagesScanned: packages.Count,
            Vulnerabilities: vulnerabilities.AsReadOnly()
        );

        _logger.LogInformation("Scan completed. {Total} packages scanned, {Vulns} vulnerabilities found",
            report.TotalPackagesScanned, report.Vulnerabilities.Count);

        return report;
    }
}