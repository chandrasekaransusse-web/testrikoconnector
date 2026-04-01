namespace TallyConnectorApp.Configuration;

public sealed class ConnectorOptions
{
    public const string SectionName = "Connector";
    public string Mode { get; set; } = "TallyHttp";
    public int SyncIntervalSeconds { get; set; } = 60;
    public string Target { get; set; } = "SqlServer";
    public TallyHttpOptions TallyHttp { get; set; } = new();
    public OdbcOptions Odbc { get; set; } = new();
    public SqlTargetOptions SqlTarget { get; set; } = new();
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
    public string LedgerGroupsQuery { get; set; } = "SELECT Guid AS GroupId, Name, ParentName, IsReserved FROM Groups";
    public string LedgersQuery { get; set; } = "SELECT Guid AS LedgerId, Name, ParentName, GstIn, Pan, Address, IsBillWiseOn FROM Ledgers";
    public string StockGroupsQuery { get; set; } = "SELECT Guid AS StockGroupId, Name, ParentName FROM StockGroups";
    public string StockItemsQuery { get; set; } = "SELECT Guid AS StockItemId, Name, StockGroupName, BaseUnit, OpeningBalance, OpeningRate FROM StockItems";
    public string CostCentersQuery { get; set; } = "SELECT Guid AS CostCenterId, Name, ParentName FROM CostCenters";
    public string VouchersQuery { get; set; } = "SELECT Guid AS VoucherId, VoucherNumber, VoucherDate, VoucherType, PartyLedgerName, Amount FROM Vouchers";
    public string VoucherEntriesQuery { get; set; } = "SELECT VoucherGuid AS VoucherId, LineNo, LedgerName, Amount, IsDebit, CostCenter, Narration FROM VoucherEntries";
}

public sealed class SqlTargetOptions
{
    public string ConnectionString { get; set; } = "Server=.;Database=TallyConnector;Trusted_Connection=True;TrustServerCertificate=True";
}
