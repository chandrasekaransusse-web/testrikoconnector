using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public interface IDataSyncTarget
{
    Task SaveAsync(IReadOnlyCollection<VoucherRecord> records, CancellationToken cancellationToken);
}
