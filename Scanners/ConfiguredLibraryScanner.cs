using Microsoft.Extensions.Options;
using SeverityChecker.Abstractions;
using SeverityChecker.Configuration;
using SeverityChecker.Models;

namespace SeverityChecker.Scanners;

public sealed class ConfiguredLibraryScanner(IOptions<ScanOptions> options) : IDependencyScanner
{
    private const string SourceName = "appsettings.json";

    public Task<IReadOnlyList<DependencyInfo>> ScanAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<DependencyInfo> dependencies = options.Value.Libraries
            .Select(library => new DependencyInfo(
                library.Name,
                library.Version,
                library.Ecosystem,
                [SourceName]))
            .ToList();
        return Task.FromResult(dependencies);
    }
}
