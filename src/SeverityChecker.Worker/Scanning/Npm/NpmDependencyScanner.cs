using System.Text.Json;
using Microsoft.Extensions.Options;
using SeverityChecker.Worker.Configuration;
using SeverityChecker.Worker.Domain;

namespace SeverityChecker.Worker.Scanning.Npm;

public sealed class NpmDependencyScanner : IDependencyScanner
{
    private readonly NpmScannerOptions _options;

    public NpmDependencyScanner(IOptions<NpmScannerOptions> options)
    {
        _options = options.Value;
    }

    public Ecosystem Ecosystem => Ecosystem.Npm;

    public bool CanHandle(string filePath) =>
        string.Equals(Path.GetFileName(filePath), "package.json", StringComparison.OrdinalIgnoreCase);

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
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(content);
        }
        catch (JsonException)
        {
            return Array.Empty<Dependency>();
        }
        using (document)
        {
            var dependencies = new List<Dependency>();
            AppendFromSection(document.RootElement, "dependencies", filePath, isDev: false, dependencies);

            if (_options.IncludeDevDependencies)
            {
                AppendFromSection(document.RootElement, "devDependencies", filePath, isDev: true, dependencies);
            }
            return dependencies;
        }
    }

    private static void AppendFromSection(
        JsonElement root,
        string sectionName,
        string filePath,
        bool isDev,
        List<Dependency> dependencies)
    {
        if (!root.TryGetProperty(sectionName, out var section) || section.ValueKind != JsonValueKind.Object)
        {
            return;
        }
        foreach (var property in section.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
            {
                continue;
            }
            var version = property.Value.GetString();
            if (string.IsNullOrWhiteSpace(version))
            {
                continue;
            }
            dependencies.Add(new Dependency
            {
                Name = property.Name,
                Version = version.Trim(),
                Ecosystem = Ecosystem.Npm,
                Source = filePath,
                Metadata = isDev
                    ? new Dictionary<string, string> { ["DependencyType"] = "dev" }
                    : new Dictionary<string, string>()
            });
        }
    }
}
