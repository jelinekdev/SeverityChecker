using Microsoft.Extensions.Options;
using Shouldly;
using SeverityChecker.Worker.Configuration;
using SeverityChecker.Worker.Domain;
using SeverityChecker.Worker.Scanning.Npm;
using Xunit;

namespace SeverityChecker.Worker.Tests.Scanning.Npm;

public class NpmDependencyScannerTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    [Theory]
    [InlineData("package.json", true)]
    [InlineData("package-lock.json", false)]
    [InlineData("requirements.txt", false)]
    public void ShouldOnlyHandlePackageJson(string fileName, bool expected)
    {
        var sut = CreateSut(includeDevDependencies: true);

        sut.CanHandle(fileName).ShouldBe(expected);
    }

    [Fact]
    public async Task ShouldDetectProductionDependency()
    {
        var filePath = WriteTempFile("""
            {
              "dependencies": {
                "lodash": "4.17.21"
              }
            }
            """);

        var sut = CreateSut(includeDevDependencies: true);

        var result = await sut.ScanAsync(filePath, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("lodash");
        result[0].Version.ShouldBe("4.17.21");
        result[0].Metadata.ShouldNotContainKey("DependencyType");
    }

    [Fact]
    public async Task ShouldDetectDevDependencyWhenEnabled()
    {
        var filePath = WriteTempFile("""
            {
              "devDependencies": {
                "jest": "29.7.0"
              }
            }
            """);

        var sut = CreateSut(includeDevDependencies: true);

        var result = await sut.ScanAsync(filePath, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("jest");
        result[0].Metadata["DependencyType"].ShouldBe("dev");
    }

    [Fact]
    public async Task ShouldSkipDevDependenciesWhenDisabled()
    {
        var filePath = WriteTempFile("""
            {
              "dependencies": {
                "lodash": "4.17.21"
              },
              "devDependencies": {
                "jest": "29.7.0"
              }
            }
            """);

        var sut = CreateSut(includeDevDependencies: false);

        var result = await sut.ScanAsync(filePath, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("lodash");
    }

    [Fact]
    public async Task ShouldReturnEmptyForInvalidJson()
    {
        var filePath = WriteTempFile("{ this is not json");

        var sut = CreateSut(includeDevDependencies: true);

        var result = await sut.ScanAsync(filePath, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenNoDependencySectionsArePresent()
    {
        var filePath = WriteTempFile("""
            {
              "name": "my-app",
              "version": "1.0.0"
            }
            """);

        var sut = CreateSut(includeDevDependencies: true);

        var result = await sut.ScanAsync(filePath, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldDetectMultipleDependenciesInSameSection()
    {
        var filePath = WriteTempFile("""
            {
              "dependencies": {
                "lodash": "4.17.21",
                "express": "4.19.2"
              }
            }
            """);

        var sut = CreateSut(includeDevDependencies: true);

        var result = await sut.ScanAsync(filePath, CancellationToken.None);

        result.Count.ShouldBe(2);
    }

    private static NpmDependencyScanner CreateSut(bool includeDevDependencies) =>
        new(Options.Create(new NpmScannerOptions { IncludeDevDependencies = includeDevDependencies }));

    private string WriteTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }
}
