using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public interface IDataSourceConnector
{
    Task<SyncBatch> FetchAsync(CancellationToken cancellationToken);
}
