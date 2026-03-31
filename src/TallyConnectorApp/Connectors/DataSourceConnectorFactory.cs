using Microsoft.Extensions.Options;
using TallyConnectorApp.Configuration;

namespace TallyConnectorApp.Connectors;

public interface IDataSourceConnectorFactory
{
    IDataSourceConnector Create();
}

public sealed class DataSourceConnectorFactory(IOptions<ConnectorOptions> options) : IDataSourceConnectorFactory
{
    private readonly ConnectorOptions _options = options.Value;

    public IDataSourceConnector Create()
    {
        return _options.Mode.ToLowerInvariant() switch
        {
            "odbc" => new OdbcConnector(_options.Odbc),
            "tallyhttp" => new TallyHttpConnector(_options.TallyHttp),
            _ => throw new InvalidOperationException($"Unsupported connector mode '{_options.Mode}'.")
        };
    }
}
