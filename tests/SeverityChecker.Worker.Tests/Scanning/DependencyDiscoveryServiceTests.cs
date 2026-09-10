using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using SeverityChecker.Worker.Configuration;
using SeverityChecker.Worker.Domain;
using SeverityChecker.Worker.Scanning;
using Xunit;

namespace SeverityChecker.Worker.Tests.Scanning;

public class DependencyDiscoveryServiceTests : IDisposable
{
    private readonly string _rootPath;

    public DependencyDiscoveryServiceTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), $"severitychecker-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_rootPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    [Fact]
    public async Task ShouldDiscoverDependenciesFromMatchingScanner()
    {
        File.WriteAllText(Path.Combine(_rootPath, "package.json"), "{}");

        var scanner = new FakeDependencyScanner(
            Ecosystem.Npm,
            "package.json",
            filePath => new[]
            {
                new Dependency { Name = "lodash", Version = "4.17.21", Ecosystem = Ecosystem.Npm, Source = filePath }
            });

        var sut = CreateSut(new[] { scanner });

        var result = await sut.DiscoverAsync(CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("lodash");
        result[0].Ecosystem.ShouldBe(Ecosystem.Npm);
    }

    [Fact]
    public async Task ShouldCombineDependenciesFromMultipleEcosystemsInSameProject()
    {
        File.WriteAllText(Path.Combine(_rootPath, "package.json"), "{}");
        File.WriteAllText(Path.Combine(_rootPath, "requirements.txt"), "");

        var npmScanner = new FakeDependencyScanner(
            Ecosystem.Npm,
            "package.json",
            filePath => new[] { new Dependency { Name = "lodash", Version = "4.17.21", Ecosystem = Ecosystem.Npm, Source = filePath } });

        var pythonScanner = new FakeDependencyScanner(
            Ecosystem.PyPi,
            "requirements.txt",
            filePath => new[] { new Dependency { Name = "requests", Version = "2.31.0", Ecosystem = Ecosystem.PyPi, Source = filePath } });

        var sut = CreateSut(new[] { npmScanner, pythonScanner });

        var result = await sut.DiscoverAsync(CancellationToken.None);

        result.Count.ShouldBe(2);
        result.ShouldContain(d => d.Ecosystem == Ecosystem.Npm);
        result.ShouldContain(d => d.Ecosystem == Ecosystem.PyPi);
    }

    [Fact]
    public async Task ShouldSkipFilesWithNoMatchingScanner()
    {
        File.WriteAllText(Path.Combine(_rootPath, "readme.txt"), "not a manifest");

        var scanner = new FakeDependencyScanner(
            Ecosystem.Npm,
            "package.json",
            _ => Array.Empty<Dependency>());

        var sut = CreateSut(new[] { scanner });

        var result = await sut.DiscoverAsync(CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldSkipExcludedDirectories()
    {
        var excludedDir = Path.Combine(_rootPath, "node_modules");
        Directory.CreateDirectory(excludedDir);
        File.WriteAllText(Path.Combine(excludedDir, "package.json"), "{}");

        var scanner = new FakeDependencyScanner(
            Ecosystem.Npm,
            "package.json",
            filePath => new[] { new Dependency { Name = "should-not-appear", Version = "1.0.0", Ecosystem = Ecosystem.Npm, Source = filePath } });

        var sut = CreateSut(new[] { scanner });

        var result = await sut.DiscoverAsync(CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldDiscoverDependenciesInNestedNonExcludedDirectories()
    {
        var nestedDir = Path.Combine(_rootPath, "backend", "src");
        Directory.CreateDirectory(nestedDir);
        File.WriteAllText(Path.Combine(nestedDir, "package.json"), "{}");

        var scanner = new FakeDependencyScanner(
            Ecosystem.Npm,
            "package.json",
            filePath => new[] { new Dependency { Name = "lodash", Version = "4.17.21", Ecosystem = Ecosystem.Npm, Source = filePath } });

        var sut = CreateSut(new[] { scanner });

        var result = await sut.DiscoverAsync(CancellationToken.None);

        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenScanPathDoesNotExist()
    {
        var scanner = new FakeDependencyScanner(Ecosystem.Npm, "package.json", _ => Array.Empty<Dependency>());
        var options = new ScannerOptions
        {
            ScanPaths = new[] { Path.Combine(_rootPath, "does-not-exist") }
        };

        var sut = new DependencyDiscoveryService(
            new[] { scanner },
            Options.Create(options),
            NullLogger<DependencyDiscoveryService>.Instance);

        var result = await sut.DiscoverAsync(CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private DependencyDiscoveryService CreateSut(IEnumerable<FakeDependencyScanner> scanners)
    {
        var options = new ScannerOptions
        {
            ScanPaths = new[] { _rootPath },
            ExcludedDirectories = new[] { ".git", "node_modules", "bin", "obj", "target", "build", "dist", ".venv" }
        };

        return new DependencyDiscoveryService(
            scanners,
            Options.Create(options),
            NullLogger<DependencyDiscoveryService>.Instance);
    }
}
