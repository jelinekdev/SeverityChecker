namespace SeverityChecker.Abstractions;

public interface IFindingStore
{
    Task<IReadOnlySet<string>> LoadNotifiedKeysAsync(CancellationToken cancellationToken);

    Task MarkAsNotifiedAsync(IEnumerable<string> keys, CancellationToken cancellationToken);
}
