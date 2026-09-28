using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SeverityChecker.Abstractions;
using SeverityChecker.Configuration;
using SeverityChecker.Models;

namespace SeverityChecker.Scanners;

public sealed partial class NuGetProjectScanner(
    IOptions<ScanOptions> options,
    ILogger<NuGetProjectScanner> logger) : IDependencyScanner
{
    private const string NuGetEcosystem = "NuGet";

    public Task<IReadOnlyList<DependencyInfo>> ScanAsync(CancellationToken cancellationToken)
    {
        var dependencies = new List<DependencyInfo>();
        foreach (var root in options.Value.ProjectRoots)
        {
            if (!Directory.Exists(root))
            {
                logger.LogWarning("Project root '{Root}' does not exist and is skipped.", root);
                continue;
            }
            foreach (var file in EnumerateProjectFiles(root))
            {
                cancellationToken.ThrowIfCancellationRequested();
                dependencies.AddRange(ReadDependenciesSafely(file));
            }
        }
        return Task.FromResult<IReadOnlyList<DependencyInfo>>(dependencies);
    }

    private static IEnumerable<string> EnumerateProjectFiles(string root) =>
        Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(IsSupportedProjectFile)
            .Where(path => !IsInBuildOutputFolder(path));

    private static bool IsSupportedProjectFile(string path) =>
        path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
        || Path.GetFileName(path).Equals("packages.config", StringComparison.OrdinalIgnoreCase);

    private static bool IsInBuildOutputFolder(string path)
    {
        var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        var segments = path.Split(separators);
        return segments.Any(segment =>
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase)
            || segment.Equals("obj", StringComparison.OrdinalIgnoreCase)
            || segment.Equals("node_modules", StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<DependencyInfo> ReadDependenciesSafely(string filePath)
    {
        try
        {
            var document = XDocument.Load(filePath);
            return Path.GetFileName(filePath).Equals("packages.config", StringComparison.OrdinalIgnoreCase)
                ? ReadPackagesConfig(document, filePath)
                : ReadProjectFile(document, filePath);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Could not read '{File}'. The file is skipped.", filePath);
            return [];
        }
    }

    private IEnumerable<DependencyInfo> ReadProjectFile(XDocument document, string filePath)
    {
        var projectDirectory = Path.GetDirectoryName(filePath)!;

        foreach (var reference in ElementsByLocalName(document, "PackageReference"))
        {
            var name = (string?)reference.Attribute("Include") ?? (string?)reference.Attribute("Update");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var version = ReadVersion(reference) ?? ResolveCentralVersion(projectDirectory, name);
            var dependency = CreateDependency(name, version, filePath);
            if (dependency is not null)
            {
                yield return dependency;
            }
        }
    }

    private IEnumerable<DependencyInfo> ReadPackagesConfig(XDocument document, string filePath)
    {
        foreach (var package in ElementsByLocalName(document, "package"))
        {
            var dependency = CreateDependency(
                (string?)package.Attribute("id"),
                (string?)package.Attribute("version"),
                filePath);
            if (dependency is not null)
            {
                yield return dependency;
            }
        }
    }

    private DependencyInfo? CreateDependency(string? name, string? version, string source)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(version))
        {
            logger.LogDebug("No version found for '{Name}' in '{Source}'. Skipped.", name, source);
            return null;
        }
        if (!ExactVersionPattern().IsMatch(version))
        {
            logger.LogWarning(
                "Version '{Version}' of '{Name}' in '{Source}' is not an exact version (range, wildcard or MSBuild variable). Skipped.",
                version, name, source);
            return null;
        }
        return new DependencyInfo(name, version, NuGetEcosystem, [source]);
    }

    private static string? ReadVersion(XElement reference) =>
        (string?)reference.Attribute("Version")
        ?? (string?)reference.Attribute("VersionOverride")
        ?? reference.Elements().FirstOrDefault(element => element.Name.LocalName == "Version")?.Value;

    private static string? ResolveCentralVersion(string projectDirectory, string packageName)
    {
        for (var directory = new DirectoryInfo(projectDirectory); directory is not null; directory = directory.Parent)
        {
            var propsPath = Path.Combine(directory.FullName, "Directory.Packages.props");
            if (!File.Exists(propsPath))
            {
                continue;
            }
            return ElementsByLocalName(XDocument.Load(propsPath), "PackageVersion")
                .Where(element => string.Equals((string?)element.Attribute("Include"), packageName, StringComparison.OrdinalIgnoreCase))
                .Select(element => (string?)element.Attribute("Version"))
                .FirstOrDefault();
        }
        return null;
    }

    private static IEnumerable<XElement> ElementsByLocalName(XDocument document, string localName) =>
        document.Descendants().Where(element => element.Name.LocalName == localName);

    [GeneratedRegex(@"^\d+(\.\d+){0,3}(-[0-9A-Za-z.\-]+)?(\+[0-9A-Za-z.\-]+)?$")]
    private static partial Regex ExactVersionPattern();
}
