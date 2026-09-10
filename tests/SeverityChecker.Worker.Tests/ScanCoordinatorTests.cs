using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SeverityChecker.Worker;
using Xunit;

namespace SeverityChecker.Worker.Tests;

public class ScanCoordinatorTests
{
    [Fact]
    public async Task ShouldCompleteWithoutThrowingWhenNoScannersAreRegistered()
    {
        var sut = new ScanCoordinator(NullLogger<ScanCoordinator>.Instance);
        var act = () => sut.RunScanCycleAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
