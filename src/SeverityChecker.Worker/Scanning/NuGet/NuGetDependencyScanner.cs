using System.Xml;
using System.Xml.Linq;
using SeverityChecker.Worker.Domain;

namespace SeverityChecker.Worker.Scanning.NuGet;

public sealed class NuGetDependencyScanner : IDependencyScanner
{
    private const string DirectoryPackagesPropsFileName = "Directory.Packages.props";

    public Ecosystem Ecosystem => Ecosystem.NuGet;

    public bool CanHandle(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        return fileName.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, DirectoryPackagesPropsFileName, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<Dependency>> ScanAsync(string filePath, CancellationToken cancellationToken)
    {
        string content;
        try
        {
            content = await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        catch (IOException)
        {
            return Array.Empty<Dependency>();
        }
        XDocument document;
        try
        {
            document = XDocument.Parse(content);
        }
        catch (XmlException)
        {
            return Array.Empty<Dependency>();
        }

        var isCentralPackageManagementFile = Path.GetFileName(filePath)
            .Equals(DirectoryPackagesPropsFileName, StringComparison.OrdinalIgnoreCase);
        var elementName = isCentralPackageManagementFile ? "PackageVersion" : "PackageReference";
        var dependencies = new List<Dependency>();
        foreach (var element in document.Descendants(elementName))
        {
            var name = element.Attribute("Include")?.Value ?? element.Attribute("Update")?.Value;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }
            var version = element.Attribute("Version")?.Value ?? element.Element("Version")?.Value;

            if (string.IsNullOrWhiteSpace(version))
            {
                continue;
            }
            dependencies.Add(new Dependency
            {
                Name = name.Trim(),
                Version = version.Trim(),
                Ecosystem = Ecosystem.NuGet,
                Source = filePath
            });
        }
        return dependencies;
    }
}
