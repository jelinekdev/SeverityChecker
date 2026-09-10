using Microsoft.Extensions.Logging;

namespace SeverityChecker.Worker;

public interface IScanCoordinator
{
    Task RunScanCycleAsync(CancellationToken cancellationToken);
}

public sealed class ScanCoordinator : IScanCoordinator
{
    private readonly ILogger<ScanCoordinator> _logger;

    public ScanCoordinator(ILogger<ScanCoordinator> logger)
    {
        _logger = logger;
    }

    public Task RunScanCycleAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scan cycle started at {Timestamp:O}", DateTimeOffset.UtcNow);
        _logger.LogInformation("No dependency scanners registered yet");
        return Task.CompletedTask;
    }
}
