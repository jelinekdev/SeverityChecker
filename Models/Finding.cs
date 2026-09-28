namespace SeverityChecker.Models;

public sealed record Finding(DependencyInfo Dependency, VulnerabilityInfo Vulnerability)
{
    public string Key =>
        $"{Dependency.Ecosystem}:{Dependency.Name}@{Dependency.Version}:{Vulnerability.Id}";
}
