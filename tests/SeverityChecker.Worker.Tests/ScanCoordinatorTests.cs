using Shouldly;
using SeverityChecker.Worker;
using Xunit;

namespace SeverityChecker.Worker.Tests;

public class ScanCoordinatorTests
{
    [Fact]
    public async Task ShouldCompleteWithoutThrowingWhenNoScannersAreRegistered()
    {
        var sut = new ScanCoordinator(Microsoft.Extensions.Logging.Abstractions.NullLogger<ScanCoordinator>.Instance);

        await Should.NotThrowAsync(() => sut.RunScanCycleAsync(CancellationToken.None));
    }
}
