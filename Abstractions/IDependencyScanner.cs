using SeverityChecker.Models;

namespace SeverityChecker.Abstractions;

public interface IDependencyScanner
{
    Task<IReadOnlyList<DependencyInfo>> ScanAsync(CancellationToken cancellationToken);
}
