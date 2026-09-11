using Shouldly;
using SeverityChecker.Worker.Domain;
using SeverityChecker.Worker.Scanning.NuGet;
using Xunit;

namespace SeverityChecker.Worker.Tests.Scanning.NuGet;

public class NuGetDependencyScannerTests : IDisposable
{
    private readonly List<string> _tempFiles = new();
    private readonly NuGetDependencyScanner _sut = new();

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
    [InlineData("MyProject.csproj", true)]
    [InlineData("Directory.Packages.props", true)]
    [InlineData("package.json", false)]
    [InlineData("requirements.txt", false)]
    public void ShouldOnlyHandleNuGetManifestFiles(string fileName, bool expected)
    {
        _sut.CanHandle(fileName).ShouldBe(expected);
    }

    [Fact]
    public async Task ShouldDetectPackageReferenceWithVersionAttribute()
    {
        var filePath = WriteTempFile(".csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
              </ItemGroup>
            </Project>
            """);

        var result = await _sut.ScanAsync(filePath, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Newtonsoft.Json");
        result[0].Version.ShouldBe("13.0.3");
        result[0].Ecosystem.ShouldBe(Ecosystem.NuGet);
    }

    [Fact]
    public async Task ShouldDetectPackageReferenceWithVersionChildElement()
    {
        var filePath = WriteTempFile(".csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog">
                  <Version>3.1.1</Version>
                </PackageReference>
              </ItemGroup>
            </Project>
            """);

        var result = await _sut.ScanAsync(filePath, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Serilog");
        result[0].Version.ShouldBe("3.1.1");
    }

    [Fact]
    public async Task ShouldDetectMultiplePackageReferencesInSameFile()
    {
        var filePath = WriteTempFile(".csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
                <PackageReference Include="Serilog" Version="3.1.1" />
              </ItemGroup>
            </Project>
            """);

        var result = await _sut.ScanAsync(filePath, CancellationToken.None);

        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldSkipPackageReferenceWithoutExplicitVersion()
    {
        var filePath = WriteTempFile(".csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" />
              </ItemGroup>
            </Project>
            """);

        var result = await _sut.ScanAsync(filePath, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldDetectPackageVersionFromDirectoryPackagesProps()
    {
        var filePath = WriteTempFile(".props", """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
              </ItemGroup>
            </Project>
            """, fixedFileName: "Directory.Packages.props");

        var result = await _sut.ScanAsync(filePath, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Newtonsoft.Json");
        result[0].Version.ShouldBe("13.0.3");
    }

    [Fact]
    public async Task ShouldReturnEmptyForInvalidXml()
    {
        var filePath = WriteTempFile(".csproj", "<Project><ItemGroup>");

        var result = await _sut.ScanAsync(filePath, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenNoPackageReferencesArePresent()
    {
        var filePath = WriteTempFile(".csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var result = await _sut.ScanAsync(filePath, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private string WriteTempFile(string extension, string content, string? fixedFileName = null)
    {
        var fileName = fixedFileName ?? $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(Path.GetTempPath(), fileName);
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }
}
