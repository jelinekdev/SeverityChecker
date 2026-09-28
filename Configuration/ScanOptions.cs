using System.ComponentModel.DataAnnotations;
using SeverityChecker.Models;

namespace SeverityChecker.Configuration;

public sealed class ScanOptions
{
    public const string SectionName = "Scan";

    [Range(1, int.MaxValue)]
    public int IntervalMinutes { get; set; } = 1440;

    public bool RunOnStartup { get; set; } = true;

    public Severity MinimumSeverity { get; set; } = Severity.Low;

    [Required]
    public string StateFilePath { get; set; } = "notified-findings.json";

    public List<string> ProjectRoots { get; set; } = [];

    public List<LibraryEntry> Libraries { get; set; } = [];
}

public sealed class LibraryEntry
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Version { get; set; } = string.Empty;

    [Required]
    public string Ecosystem { get; set; } = "NuGet";
}
