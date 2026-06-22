using Cronos;
using Microsoft.Extensions.Options;
using SeverityChecker.Core.Interfaces;
using SeverityChecker.Core.Options;

namespace SeverityChecker;

public sealed class Worker : BackgroundService
{
    private readonly IScanService _scanService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<Worker> _logger;
    private readonly ScanOptions _scanOptions;

    public Worker(
        IScanService scanService,
        INotificationService notificationService,
        ILogger<Worker> logger,
        IOptions<ScanOptions> scanOptions)
    {
        _scanService = scanService;
        _notificationService = notificationService;
        _logger = logger;
        _scanOptions = scanOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SeverityChecker Worker started");
        _logger.LogInformation("Cron schedule: {Cron}", _scanOptions.CronExpression);

        var expression = CronExpression.Parse(_scanOptions.CronExpression);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var next = expression.GetNextOccurrence(now, TimeZoneInfo.Utc);

            if (next is null)
            {
                _logger.LogError("Could not determine next cron occurrence, stopping worker");
                break;
            }

            var delay = next.Value - now;
            _logger.LogInformation("Next scan scheduled at {Next} (in {Delay})", next, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RunScanAsync(stoppingToken);
        }

        _logger.LogInformation("SeverityChecker Worker stopped");
    }

    private async Task RunScanAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Running scheduled scan...");

            var report = await _scanService.ScanAsync(stoppingToken);
            await _notificationService.SendReportAsync(report, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Scan cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during scan");
        }
    }
}