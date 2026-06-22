using SeverityChecker.Core.Models;

namespace SeverityChecker.Core.Interfaces;

public interface IScanService
{
    Task<ScanReport> ScanAsync(CancellationToken cancellationToken = default);
}