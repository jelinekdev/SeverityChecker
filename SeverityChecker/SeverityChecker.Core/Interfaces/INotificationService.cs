using SeverityChecker.Core.Models;
namespace SeverityChecker.Core.Interfaces;

public interface INotificationService
{
    Task SendReportAsync(ScanReport report, CancellationToken cancellationToken = default);
}