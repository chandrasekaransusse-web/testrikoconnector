namespace TallyConnectorApp.Models;

public sealed class SyncBatch
{
    public List<LedgerGroup> LedgerGroups { get; init; } = [];
    public List<LedgerMaster> Ledgers { get; init; } = [];
    public List<StockGroup> StockGroups { get; init; } = [];
    public List<StockItem> StockItems { get; init; } = [];
    public List<CostCenter> CostCenters { get; init; } = [];
    public List<VoucherMaster> Vouchers { get; init; } = [];
    public List<VoucherEntry> VoucherEntries { get; init; } = [];
    public DateTime SyncedAtUtc { get; init; } = DateTime.UtcNow;
}

public sealed record LedgerGroup(string GroupId, string Name, string? ParentGroupName, bool IsReserved);

public sealed record LedgerMaster(
    string LedgerId,
    string Name,
    string? ParentGroupName,
    string? GstIn,
    string? Pan,
    string? Address,
    bool IsBillWiseOn);

public sealed record StockGroup(string StockGroupId, string Name, string? ParentStockGroupName);

public sealed record StockItem(
    string StockItemId,
    string Name,
    string? StockGroupName,
    string? BaseUnit,
    decimal OpeningBalance,
    decimal OpeningRate);

public sealed record CostCenter(string CostCenterId, string Name, string? ParentCostCenterName);

public sealed record VoucherMaster(
    string VoucherId,
    string VoucherNumber,
    DateTime VoucherDate,
    string VoucherType,
    string? PartyLedgerName,
    decimal TotalAmount,
    string Source);

public sealed record VoucherEntry(
    string VoucherId,
    int LineNo,
    string LedgerName,
    decimal Amount,
    bool IsDebit,
    string? CostCenter,
    string? Narration);
