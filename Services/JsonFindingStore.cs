using System.Text.Json;
using Microsoft.Extensions.Options;
using SeverityChecker.Abstractions;
using SeverityChecker.Configuration;

namespace SeverityChecker.Services;

public sealed class JsonFindingStore(IOptions<ScanOptions> options) : IFindingStore
{
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    private string FilePath => Path.IsPathRooted(options.Value.StateFilePath)
        ? options.Value.StateFilePath
        : Path.Combine(AppContext.BaseDirectory, options.Value.StateFilePath);

    public async Task<IReadOnlySet<string>> LoadNotifiedKeysAsync(CancellationToken cancellationToken)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            return await ReadKeysAsync(cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task MarkAsNotifiedAsync(IEnumerable<string> keys, CancellationToken cancellationToken)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var allKeys = new HashSet<string>(await ReadKeysAsync(cancellationToken));
            allKeys.UnionWith(keys);

            await using var stream = File.Create(FilePath);
            await JsonSerializer.SerializeAsync(stream, allKeys.Order().ToList(), cancellationToken: cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<HashSet<string>> ReadKeysAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(FilePath))
        {
            return [];
        }
        await using var stream = File.OpenRead(FilePath);
        var keys = await JsonSerializer.DeserializeAsync<List<string>>(stream, cancellationToken: cancellationToken);
        return keys is null ? [] : new HashSet<string>(keys);
    }
}
