using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TallyConnectorApp.Configuration;
using TallyConnectorApp.Connectors;

namespace TallyConnectorApp;

public sealed class TallySyncWorker(
    ILogger<TallySyncWorker> logger,
    IDataSourceConnectorFactory sourceFactory,
    IDataSyncTarget syncTarget,
    IOptions<ConnectorOptions> options) : BackgroundService
{
    private readonly ConnectorOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Tally connector started in {Mode} mode with interval {Interval}s.",
            _options.Mode,
            _options.SyncIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var source = sourceFactory.Create();
                var vouchers = await source.FetchVouchersAsync(stoppingToken);
                await syncTarget.SaveAsync(vouchers, stoppingToken);

                logger.LogInformation("Synced {Count} vouchers at {UtcNow}.", vouchers.Count, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Sync cycle failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, _options.SyncIntervalSeconds)), stoppingToken);
        }
    }
}
