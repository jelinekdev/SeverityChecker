using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SeverityChecker.Worker.Domain;
using SeverityChecker.Worker.Scanning;
using Xunit;

namespace SeverityChecker.Worker.Tests;

public class ScanCoordinatorTests
{
    [Fact]
    public async Task ShouldCompleteWithoutThrowingWhenNoDependenciesAreDiscovered()
    {
        var sut = new ScanCoordinator(
            new FakeDependencyDiscoveryService(Array.Empty<Dependency>()),
            NullLogger<ScanCoordinator>.Instance);

        await Should.NotThrowAsync(() => sut.RunScanCycleAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ShouldCompleteWithoutThrowingWhenDependenciesAreDiscovered()
    {
        var discovered = new[]
        {
            new Dependency { Name = "lodash", Version = "4.17.21", Ecosystem = Ecosystem.Npm, Source = "package.json" }
        };

        var sut = new ScanCoordinator(
            new FakeDependencyDiscoveryService(discovered),
            NullLogger<ScanCoordinator>.Instance);

        await Should.NotThrowAsync(() => sut.RunScanCycleAsync(CancellationToken.None));
    }

    private sealed class FakeDependencyDiscoveryService : IDependencyDiscoveryService
    {
        private readonly IReadOnlyList<Dependency> _dependencies;

        public FakeDependencyDiscoveryService(IReadOnlyList<Dependency> dependencies)
        {
            _dependencies = dependencies;
        }

        public Task<IReadOnlyList<Dependency>> DiscoverAsync(CancellationToken cancellationToken) =>
            Task.FromResult(_dependencies);
    }
}
