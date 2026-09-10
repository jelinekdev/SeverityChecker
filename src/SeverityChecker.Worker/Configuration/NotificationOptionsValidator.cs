using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace SeverityChecker.Worker.Configuration;

public sealed partial class NotificationOptionsValidator : IValidateOptions<NotificationOptions>
{
    public ValidateOptionsResult Validate(string? name, NotificationOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }
        var failures = new List<string>();
        if (string.IsNullOrWhiteSpace(options.Recipient) || !EmailPattern().IsMatch(options.Recipient))
        {
            failures.Add($"{NotificationOptions.SectionName}:{nameof(NotificationOptions.Recipient)} must be a valid email address when notifications are enabled.");
        }
        if (string.IsNullOrWhiteSpace(options.Smtp.Host))
        {
            failures.Add($"{NotificationOptions.SectionName}:Smtp:{nameof(SmtpOptions.Host)} must not be empty when notifications are enabled.");
        }
        if (options.Smtp.Port is <= 0 or > 65535)
        {
            failures.Add($"{NotificationOptions.SectionName}:Smtp:{nameof(SmtpOptions.Port)} must be between 1 and 65535.");
        }
        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
