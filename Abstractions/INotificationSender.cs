using SeverityChecker.Models;

namespace SeverityChecker.Abstractions;

public interface INotificationSender
{
    Task SendAsync(ScanReport report, CancellationToken cancellationToken);
}
