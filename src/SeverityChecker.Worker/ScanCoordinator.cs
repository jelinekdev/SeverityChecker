using Microsoft.Extensions.Logging;
using SeverityChecker.Worker.Scanning;

namespace SeverityChecker.Worker;

public interface IScanCoordinator
{
    Task RunScanCycleAsync(CancellationToken cancellationToken);
}

public sealed class ScanCoordinator : IScanCoordinator
{
    private readonly IDependencyDiscoveryService _discoveryService;
    private readonly ILogger<ScanCoordinator> _logger;

    public ScanCoordinator(IDependencyDiscoveryService discoveryService, ILogger<ScanCoordinator> logger)
    {
        _discoveryService = discoveryService;
        _logger = logger;
    }

    public async Task RunScanCycleAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scan cycle started at {Timestamp:O}", DateTimeOffset.UtcNow);

        var dependencies = await _discoveryService.DiscoverAsync(cancellationToken);

        _logger.LogInformation("Discovered {Count} dependency(ies) across all scan paths", dependencies.Count);

        // Vulnerability lookup, severity evaluation and notification dispatch
        // are composed here in later phases.
    }
}
