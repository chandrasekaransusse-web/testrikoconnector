using Microsoft.Extensions.Options;
using TallyConnectorApp.Configuration;

namespace TallyConnectorApp.Connectors;

public interface IDataSyncTargetFactory
{
    IDataSyncTarget Create();
}

public sealed class DataSyncTargetFactory(IOptions<ConnectorOptions> options) : IDataSyncTargetFactory
{
    private readonly ConnectorOptions _options = options.Value;

    public IDataSyncTarget Create()
    {
        return _options.Target.ToLowerInvariant() switch
        {
            "json" => new JsonFileSyncTarget(),
            "sqlserver" => new SqlServerSyncTarget(_options.SqlTarget),
            _ => throw new InvalidOperationException($"Unsupported target '{_options.Target}'.")
        };
    }
}
