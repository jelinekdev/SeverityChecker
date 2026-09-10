using SeverityChecker.Worker.Domain;

namespace SeverityChecker.Worker.Scanning;

/// <summary>
/// Parses a single manifest file of a specific ecosystem into normalized dependencies.
/// Implementations must not perform directory traversal themselves; that is the
/// responsibility of <see cref="IDependencyDiscoveryService"/>.
/// </summary>
public interface IDependencyScanner
{
    Ecosystem Ecosystem { get; }

    bool CanHandle(string filePath);

    Task<IReadOnlyList<Dependency>> ScanAsync(string filePath, CancellationToken cancellationToken);
}
