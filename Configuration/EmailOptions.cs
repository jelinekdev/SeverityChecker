namespace SeverityChecker.Configuration;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; } = true;

    public List<string> Recipients { get; set; } = [];
}
