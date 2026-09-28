namespace SeverityChecker.Models;

public sealed record DependencyInfo(
    string Name,
    string Version,
    string Ecosystem,
    IReadOnlyList<string> Sources);
