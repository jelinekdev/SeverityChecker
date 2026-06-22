using System.Xml.Linq;
using SeverityChecker.Core.Models;

namespace SeverityChecker.Infrastructure.Parsers;

public sealed class NuGetPackageParser
{
    public IReadOnlyList<PackageInfo> ParseProjectFile(string csprojPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(csprojPath);

        if (!File.Exists(csprojPath))
            throw new FileNotFoundException($"Project file not found: {csprojPath}");

        var document = XDocument.Load(csprojPath);

        return document
            .Descendants("PackageReference")
            .Select(element => new PackageInfo(
                Name: element.Attribute("Include")?.Value ?? string.Empty,
                Version: element.Attribute("Version")?.Value ?? string.Empty,
                ProjectPath: csprojPath
            ))
            .Where(p => !string.IsNullOrWhiteSpace(p.Name) && !string.IsNullOrWhiteSpace(p.Version))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<PackageInfo> ParseDirectory(string basePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);

        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"Directory not found: {basePath}");

        return Directory
            .GetFiles(basePath, "*.csproj", SearchOption.AllDirectories)
            .SelectMany(ParseProjectFile)
            .DistinctBy(p => new { p.Name, p.Version })
            .ToList()
            .AsReadOnly();
    }
}