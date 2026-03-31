using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public interface IDataSourceConnector
{
    Task<IReadOnlyCollection<VoucherRecord>> FetchVouchersAsync(CancellationToken cancellationToken);
}
