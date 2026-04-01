using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public interface IDataSyncTarget
{
    Task SaveAsync(SyncBatch batch, CancellationToken cancellationToken);
}
