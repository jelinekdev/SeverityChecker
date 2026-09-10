using Shouldly;
using SeverityChecker.Worker.Configuration;
using Xunit;

namespace SeverityChecker.Worker.Tests.Configuration;

public class ScannerOptionsValidatorTests
{
    private readonly ScannerOptionsValidator _sut = new();

    [Fact]
    public void ShouldSucceedForValidConfiguration()
    {
        var options = new ScannerOptions
        {
            ScanIntervalMinutes = 30,
            ScanPaths = new[] { @"E:\dev\ProjectA" }
        };

        var result = _sut.Validate(name: null, options);

        result.Succeeded.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ShouldFailWhenScanIntervalIsNotPositive(int interval)
    {
        var options = new ScannerOptions
        {
            ScanIntervalMinutes = interval,
            ScanPaths = new[] { @"E:\dev\ProjectA" }
        };

        var result = _sut.Validate(name: null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain(f => f.Contains(nameof(ScannerOptions.ScanIntervalMinutes)));
    }

    [Fact]
    public void ShouldFailWhenNoScanPathsAreConfigured()
    {
        var options = new ScannerOptions
        {
            ScanIntervalMinutes = 60,
            ScanPaths = Array.Empty<string>()
        };

        var result = _sut.Validate(name: null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain(f => f.Contains(nameof(ScannerOptions.ScanPaths)));
    }

    [Fact]
    public void ShouldFailWhenScanPathsContainsBlankEntry()
    {
        var options = new ScannerOptions
        {
            ScanIntervalMinutes = 60,
            ScanPaths = new[] { @"E:\dev\ProjectA", "   " }
        };

        var result = _sut.Validate(name: null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain(f => f.Contains(nameof(ScannerOptions.ScanPaths)));
    }
}
