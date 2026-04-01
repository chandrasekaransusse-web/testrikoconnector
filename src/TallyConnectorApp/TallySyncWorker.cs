using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TallyConnectorApp.Configuration;
using TallyConnectorApp.Connectors;

namespace TallyConnectorApp;

public sealed class TallySyncWorker(
    ILogger<TallySyncWorker> logger,
    IDataSourceConnectorFactory sourceFactory,
    IDataSyncTargetFactory targetFactory,
    IOptions<ConnectorOptions> options) : BackgroundService
{
    private readonly ConnectorOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Tally connector started in {Mode} -> {Target} mode with interval {Interval}s.",
            _options.Mode, _options.Target, _options.SyncIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var source = sourceFactory.Create();
                var target = targetFactory.Create();
                var batch = await source.FetchAsync(stoppingToken);
                await target.SaveAsync(batch, stoppingToken);

                logger.LogInformation("Synced Groups:{Groups} Ledgers:{Ledgers} StockGroups:{StockGroups} StockItems:{StockItems} CostCenters:{CostCenters} Vouchers:{Vouchers} Entries:{Entries} at {UtcNow}.",
                    batch.LedgerGroups.Count,
                    batch.Ledgers.Count,
                    batch.StockGroups.Count,
                    batch.StockItems.Count,
                    batch.CostCenters.Count,
                    batch.Vouchers.Count,
                    batch.VoucherEntries.Count,
                    DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Sync cycle failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, _options.SyncIntervalSeconds)), stoppingToken);
        }
    }
}
