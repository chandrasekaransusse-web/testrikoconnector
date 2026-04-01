using System.Text.Json;
using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public sealed class JsonFileSyncTarget : IDataSyncTarget
{
    public async Task SaveAsync(SyncBatch batch, CancellationToken cancellationToken)
    {
        var outputDir = Path.Combine(AppContext.BaseDirectory, "sync-output");
        Directory.CreateDirectory(outputDir);

        var filePath = Path.Combine(outputDir, $"tally-batch-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, batch, cancellationToken: cancellationToken);
    }
}
