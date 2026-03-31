namespace TallyConnectorApp.Configuration;

public sealed class ConnectorOptions
{
    public const string SectionName = "Connector";

    public string Mode { get; set; } = "TallyHttp";

    public int SyncIntervalSeconds { get; set; } = 60;

    public TallyHttpOptions TallyHttp { get; set; } = new();

    public OdbcOptions Odbc { get; set; } = new();
}

public sealed class TallyHttpOptions
{
    public string Url { get; set; } = "http://localhost:9000";

    public int TimeoutSeconds { get; set; } = 30;

    public string CompanyName { get; set; } = "";
}

public sealed class OdbcOptions
{
    public string ConnectionString { get; set; } = "";

    public string VoucherQuery { get; set; } = "SELECT TOP 100 * FROM Vouchers";
}
