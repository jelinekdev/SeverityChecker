using SeverityChecker.Worker.Domain;

namespace SeverityChecker.Worker.Scanning;

public interface IDependencyDiscoveryService
{
    Task<IReadOnlyList<Dependency>> DiscoverAsync(CancellationToken cancellationToken);
}
