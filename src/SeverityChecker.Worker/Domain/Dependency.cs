namespace SeverityChecker.Worker.Domain;

public sealed record Dependency
{
    public required string Name { get; init; }

    public required string Version { get; init; }

    public required Ecosystem Ecosystem { get; init; }

    public required string Source { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}
