using Microsoft.Extensions.Options;
using SeverityChecker.Configuration;
using SeverityChecker.Services;

namespace SeverityChecker.Workers;

public sealed class SeverityCheckWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ScanOptions> options,
    ILogger<SeverityCheckWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(options.Value.IntervalMinutes);
        logger.LogInformation("SeverityChecker started. Interval: {Interval}.", interval);
        if (options.Value.RunOnStartup)
        {
            await RunScanSafelyAsync(stoppingToken);
        }
        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunScanSafelyAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("SeverityChecker is stopping.");
        }
    }

    private async Task RunScanSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var scanService = scope.ServiceProvider.GetRequiredService<VulnerabilityScanService>();
            await scanService.RunAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Scan run failed.");
        }
    }
}
