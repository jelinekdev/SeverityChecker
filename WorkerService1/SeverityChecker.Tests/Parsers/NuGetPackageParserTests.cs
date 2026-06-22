using SeverityChecker.Infrastructure.Parsers;
using NUnit.Framework;

namespace SeverityChecker.Tests.Parsers;

[TestFixture]
public sealed class NuGetPackageParserTests
{
    private NuGetPackageParser _parser = null!;
    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new NuGetPackageParser();
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    [Test]
    public void ParseProjectFile_ValidCsproj_ReturnsPackages()
    {
        // Arrange
        var csprojPath = CreateTempCsproj("""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
                <PackageReference Include="Serilog" Version="3.0.0" />
              </ItemGroup>
            </Project>
            """);

        // Act
        var result = _parser.ParseProjectFile(csprojPath);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("Newtonsoft.Json"));
        Assert.That(result[0].Version, Is.EqualTo("13.0.1"));
        Assert.That(result[1].Name, Is.EqualTo("Serilog"));
    }

    [Test]
    public void ParseProjectFile_EmptyCsproj_ReturnsEmptyList()
    {
        // Arrange
        var csprojPath = CreateTempCsproj("""
            <Project Sdk="Microsoft.NET.Sdk">
            </Project>
            """);

        // Act
        var result = _parser.ParseProjectFile(csprojPath);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseProjectFile_FileNotFound_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            _parser.ParseProjectFile("nonexistent.csproj"));
    }

    [Test]
    public void ParseDirectory_MultipleProjects_ReturnsMergedPackages()
    {
        // Arrange
        CreateTempCsproj("""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
              </ItemGroup>
            </Project>
            """, "Project1.csproj");

        CreateTempCsproj("""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog" Version="3.0.0" />
              </ItemGroup>
            </Project>
            """, "Project2.csproj");

        // Act
        var result = _parser.ParseDirectory(_tempDirectory);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public void ParseDirectory_DuplicatePackages_ReturnsDistinct()
    {
        // Arrange
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
              </ItemGroup>
            </Project>
            """;

        CreateTempCsproj(csprojContent, "Project1.csproj");
        CreateTempCsproj(csprojContent, "Project2.csproj");

        // Act
        var result = _parser.ParseDirectory(_tempDirectory);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }

    private string CreateTempCsproj(string content, string fileName = "Test.csproj")
    {
        var path = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(path, content);
        return path;
    }
}