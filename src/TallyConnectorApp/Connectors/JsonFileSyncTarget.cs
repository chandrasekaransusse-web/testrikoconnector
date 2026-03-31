using System.Text.Json;
using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public sealed class JsonFileSyncTarget : IDataSyncTarget
{
    public async Task SaveAsync(IReadOnlyCollection<VoucherRecord> records, CancellationToken cancellationToken)
    {
        var outputDir = Path.Combine(AppContext.BaseDirectory, "sync-output");
        Directory.CreateDirectory(outputDir);

        var filePath = Path.Combine(outputDir, $"vouchers-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, records, cancellationToken: cancellationToken);
    }
}
