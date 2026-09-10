using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SeverityChecker.Worker.Configuration;
using SeverityChecker.Worker.Domain;

namespace SeverityChecker.Worker.Scanning;

public sealed class DependencyDiscoveryService : IDependencyDiscoveryService
{
    private readonly IReadOnlyList<IDependencyScanner> _scanners;
    private readonly ScannerOptions _options;
    private readonly ILogger<DependencyDiscoveryService> _logger;

    public DependencyDiscoveryService(
        IEnumerable<IDependencyScanner> scanners,
        IOptions<ScannerOptions> options,
        ILogger<DependencyDiscoveryService> logger)
    {
        _scanners = scanners.ToList();
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Dependency>> DiscoverAsync(CancellationToken cancellationToken)
    {
        var dependencies = new List<Dependency>();

        foreach (var scanPath in _options.ScanPaths)
        {
            if (!Directory.Exists(scanPath))
            {
                _logger.LogWarning("Scan path does not exist: {ScanPath}", scanPath);
                continue;
            }

            foreach (var filePath in EnumerateManifestFiles(scanPath, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var scanner = _scanners.FirstOrDefault(s => s.CanHandle(filePath));
                if (scanner is null)
                {
                    continue;
                }

                _logger.LogInformation("Detected {Ecosystem} manifest at {FilePath}", scanner.Ecosystem, filePath);

                var found = await scanner.ScanAsync(filePath, cancellationToken);
                dependencies.AddRange(found);
            }
        }

        return dependencies;
    }

    private IEnumerable<string> EnumerateManifestFiles(string rootPath, CancellationToken cancellationToken)
    {
        var directoriesToVisit = new Stack<string>();
        directoriesToVisit.Push(rootPath);

        while (directoriesToVisit.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentDirectory = directoriesToVisit.Pop();

            List<string> subDirectories;
            List<string> files;

            try
            {
                subDirectories = Directory.EnumerateDirectories(currentDirectory).ToList();
                files = Directory.EnumerateFiles(currentDirectory).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Skipping directory due to missing permissions: {Directory}", currentDirectory);
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            foreach (var file in files)
            {
                yield return file;
            }

            foreach (var directory in subDirectories)
            {
                var directoryName = Path.GetFileName(directory);
                if (_options.ExcludedDirectories.Contains(directoryName, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                directoriesToVisit.Push(directory);
            }
        }
    }
}
