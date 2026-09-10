using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SeverityChecker.Worker.Configuration;

namespace SeverityChecker.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IScanCoordinator _scanCoordinator;
    private readonly ScannerOptions _options;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IScanCoordinator scanCoordinator,
        IOptions<ScannerOptions> options,
        ILogger<Worker> logger)
    {
        _scanCoordinator = scanCoordinator;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "SeverityChecker worker starting. Scan interval: {IntervalMinutes} minute(s)",
            _options.ScanIntervalMinutes);
        
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.ScanIntervalMinutes));
        await RunSafelyAsync(stoppingToken);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunSafelyAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("SeverityChecker worker is shutting down");
        }
    }

    private async Task RunSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _scanCoordinator.RunScanCycleAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scan cycle failed unexpectedly");
        }
    }
}
