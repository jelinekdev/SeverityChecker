namespace SeverityChecker.Core.Models;

public sealed record PackageInfo(
    string Name,
    string Version,
    string ProjectPath
);