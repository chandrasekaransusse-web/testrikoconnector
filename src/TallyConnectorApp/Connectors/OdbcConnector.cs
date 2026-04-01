using System.Data.Odbc;
using TallyConnectorApp.Configuration;
using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public sealed class OdbcConnector(OdbcOptions options) : IDataSourceConnector
{
    public async Task<SyncBatch> FetchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("ODBC connection string is required when mode is set to Odbc.");
        }

        await using var connection = new OdbcConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        return new SyncBatch
        {
            LedgerGroups = await ReadGroupsAsync(connection, options.LedgerGroupsQuery, cancellationToken),
            Ledgers = await ReadLedgersAsync(connection, options.LedgersQuery, cancellationToken),
            StockGroups = await ReadStockGroupsAsync(connection, options.StockGroupsQuery, cancellationToken),
            StockItems = await ReadStockItemsAsync(connection, options.StockItemsQuery, cancellationToken),
            CostCenters = await ReadCostCentersAsync(connection, options.CostCentersQuery, cancellationToken),
            Vouchers = await ReadVouchersAsync(connection, options.VouchersQuery, cancellationToken),
            VoucherEntries = await ReadVoucherEntriesAsync(connection, options.VoucherEntriesQuery, cancellationToken)
        };
    }

    private static async Task<List<LedgerGroup>> ReadGroupsAsync(OdbcConnection c, string sql, CancellationToken ct)
    {
        var result = new List<LedgerGroup>();
        await using var cmd = new OdbcCommand(sql, c);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            result.Add(new LedgerGroup(r["GroupId"]?.ToString() ?? Guid.NewGuid().ToString("N"), r["Name"]?.ToString() ?? "UNKNOWN", r["ParentName"]?.ToString(), bool.TryParse(r["IsReserved"]?.ToString(), out var x) && x));
        return result;
    }

    private static async Task<List<LedgerMaster>> ReadLedgersAsync(OdbcConnection c, string sql, CancellationToken ct)
    {
        var result = new List<LedgerMaster>();
        await using var cmd = new OdbcCommand(sql, c);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            result.Add(new LedgerMaster(r["LedgerId"]?.ToString() ?? Guid.NewGuid().ToString("N"), r["Name"]?.ToString() ?? "UNKNOWN", r["ParentName"]?.ToString(), r["GstIn"]?.ToString(), r["Pan"]?.ToString(), r["Address"]?.ToString(), bool.TryParse(r["IsBillWiseOn"]?.ToString(), out var x) && x));
        return result;
    }

    private static async Task<List<StockGroup>> ReadStockGroupsAsync(OdbcConnection c, string sql, CancellationToken ct)
    {
        var result = new List<StockGroup>();
        await using var cmd = new OdbcCommand(sql, c);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            result.Add(new StockGroup(r["StockGroupId"]?.ToString() ?? Guid.NewGuid().ToString("N"), r["Name"]?.ToString() ?? "UNKNOWN", r["ParentName"]?.ToString()));
        return result;
    }

    private static async Task<List<StockItem>> ReadStockItemsAsync(OdbcConnection c, string sql, CancellationToken ct)
    {
        var result = new List<StockItem>();
        await using var cmd = new OdbcCommand(sql, c);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            _ = decimal.TryParse(r["OpeningBalance"]?.ToString(), out var ob);
            _ = decimal.TryParse(r["OpeningRate"]?.ToString(), out var orate);
            result.Add(new StockItem(r["StockItemId"]?.ToString() ?? Guid.NewGuid().ToString("N"), r["Name"]?.ToString() ?? "UNKNOWN", r["StockGroupName"]?.ToString(), r["BaseUnit"]?.ToString(), ob, orate));
        }
        return result;
    }

    private static async Task<List<CostCenter>> ReadCostCentersAsync(OdbcConnection c, string sql, CancellationToken ct)
    {
        var result = new List<CostCenter>();
        await using var cmd = new OdbcCommand(sql, c);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            result.Add(new CostCenter(r["CostCenterId"]?.ToString() ?? Guid.NewGuid().ToString("N"), r["Name"]?.ToString() ?? "UNKNOWN", r["ParentName"]?.ToString()));
        return result;
    }

    private static async Task<List<VoucherMaster>> ReadVouchersAsync(OdbcConnection c, string sql, CancellationToken ct)
    {
        var result = new List<VoucherMaster>();
        await using var cmd = new OdbcCommand(sql, c);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            _ = DateTime.TryParse(r["VoucherDate"]?.ToString(), out var d);
            _ = decimal.TryParse(r["Amount"]?.ToString(), out var a);
            result.Add(new VoucherMaster(r["VoucherId"]?.ToString() ?? Guid.NewGuid().ToString("N"), r["VoucherNumber"]?.ToString() ?? "UNKNOWN", d, r["VoucherType"]?.ToString() ?? "UNKNOWN", r["PartyLedgerName"]?.ToString(), a, "ODBC"));
        }
        return result;
    }

    private static async Task<List<VoucherEntry>> ReadVoucherEntriesAsync(OdbcConnection c, string sql, CancellationToken ct)
    {
        var result = new List<VoucherEntry>();
        await using var cmd = new OdbcCommand(sql, c);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            _ = int.TryParse(r["LineNo"]?.ToString(), out var line);
            _ = decimal.TryParse(r["Amount"]?.ToString(), out var amount);
            result.Add(new VoucherEntry(r["VoucherId"]?.ToString() ?? string.Empty, line, r["LedgerName"]?.ToString() ?? "UNKNOWN", amount, bool.TryParse(r["IsDebit"]?.ToString(), out var x) && x, r["CostCenter"]?.ToString(), r["Narration"]?.ToString()));
        }
        return result;
    }
}
