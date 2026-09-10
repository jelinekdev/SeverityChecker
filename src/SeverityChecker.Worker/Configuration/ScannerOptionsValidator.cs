using Microsoft.Extensions.Options;

namespace SeverityChecker.Worker.Configuration;

public sealed class ScannerOptionsValidator : IValidateOptions<ScannerOptions>
{
    public ValidateOptionsResult Validate(string? name, ScannerOptions options)
    {
        var failures = new List<string>();
        if (options.ScanIntervalMinutes <= 0)
        {
            failures.Add($"{ScannerOptions.SectionName}:{nameof(ScannerOptions.ScanIntervalMinutes)} must be greater than zero.");
        }
        if (options.ScanPaths.Count == 0)
        {
            failures.Add($"{ScannerOptions.SectionName}:{nameof(ScannerOptions.ScanPaths)} must contain at least one path.");
        }

        foreach (var path in options.ScanPaths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                failures.Add($"{ScannerOptions.SectionName}:{nameof(ScannerOptions.ScanPaths)} must not contain empty entries.");
            }
        }
        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
