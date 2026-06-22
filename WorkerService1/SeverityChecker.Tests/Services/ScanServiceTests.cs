using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using SeverityChecker.Core.Interfaces;
using SeverityChecker.Core.Models;
using SeverityChecker.Core.Options;
using SeverityChecker.Infrastructure.Parsers;
using SeverityChecker.Infrastructure.Services;

namespace SeverityChecker.Tests.Services;

[TestFixture]
public sealed class ScanServiceTests
{
    private ScanService _sut = null!;
    private IVulnerabilitySource _vulnerabilitySource = null!;
    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _vulnerabilitySource = Substitute.For<IVulnerabilitySource>();
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        var options = Options.Create(new ScanOptions
        {
            ProjectsBasePath = _tempDirectory,
            SeverityFilter = ["CRITICAL", "HIGH"]
        });

        _sut = new ScanService(
            new NuGetPackageParser(),
            _vulnerabilitySource,
            NullLogger<ScanService>.Instance,
            options
        );
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    [Test]
    public async Task ScanAsyncNoProjectsReturnsEmptyReport()
    {
        var report = await _sut.ScanAsync();

        Assert.That(report.TotalPackagesScanned, Is.EqualTo(0));
        Assert.That(report.HasVulnerabilities, Is.False);
    }

    [Test]
    public async Task ScanAsyncWithVulnerablePackageReturnsReport()
    {
        File.WriteAllText(Path.Combine(_tempDirectory, "Test.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="12.0.1" />
              </ItemGroup>
            </Project>
            """);

        var package = new PackageInfo("Newtonsoft.Json", "12.0.1", "Test.csproj");
        var vulnerability = new VulnerabilityResult(
            Package: package,
            CveId: "GHSA-test-0001",
            Summary: "Test vulnerability",
            Severity: "HIGH",
            FixedVersion: "13.0.1",
            ReferenceUrl: "https://example.com"
        );

        _vulnerabilitySource
            .GetVulnerabilitiesAsync(Arg.Any<PackageInfo>(), Arg.Any<CancellationToken>())
            .Returns([vulnerability]);

        var report = await _sut.ScanAsync();

        Assert.That(report.TotalPackagesScanned, Is.EqualTo(1));
        Assert.That(report.HasVulnerabilities, Is.True);
        Assert.That(report.Vulnerabilities[0].CveId, Is.EqualTo("GHSA-test-0001"));
    }
}