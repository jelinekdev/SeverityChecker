using SeverityChecker.Worker.Domain;

namespace SeverityChecker.Worker.Tests.Scanning;

/// <summary>
/// Minimal test double for IDependencyScanner. Matches files by exact name
/// (e.g. "package.json") and returns a fixed, caller-provided dependency list.
/// </summary>
public sealed class FakeDependencyScanner : SeverityChecker.Worker.Scanning.IDependencyScanner
{
    private readonly string _matchingFileName;
    private readonly Func<string, IReadOnlyList<Dependency>> _resultFactory;

    public FakeDependencyScanner(Ecosystem ecosystem, string matchingFileName, Func<string, IReadOnlyList<Dependency>> resultFactory)
    {
        Ecosystem = ecosystem;
        _matchingFileName = matchingFileName;
        _resultFactory = resultFactory;
    }

    public Ecosystem Ecosystem { get; }

    public bool CanHandle(string filePath) =>
        string.Equals(Path.GetFileName(filePath), _matchingFileName, StringComparison.OrdinalIgnoreCase);

    public Task<IReadOnlyList<Dependency>> ScanAsync(string filePath, CancellationToken cancellationToken) =>
        Task.FromResult(_resultFactory(filePath));
}
