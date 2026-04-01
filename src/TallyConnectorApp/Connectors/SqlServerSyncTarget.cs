using Microsoft.Data.SqlClient;
using TallyConnectorApp.Configuration;
using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public sealed class SqlServerSyncTarget(SqlTargetOptions options) : IDataSyncTarget
{
    public async Task SaveAsync(SyncBatch batch, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var tx = connection.BeginTransaction();

        foreach (var x in batch.LedgerGroups) await UpsertGroupAsync(connection, tx, x, cancellationToken);
        foreach (var x in batch.Ledgers) await UpsertLedgerAsync(connection, tx, x, cancellationToken);
        foreach (var x in batch.StockGroups) await UpsertStockGroupAsync(connection, tx, x, cancellationToken);
        foreach (var x in batch.StockItems) await UpsertStockItemAsync(connection, tx, x, cancellationToken);
        foreach (var x in batch.CostCenters) await UpsertCostCenterAsync(connection, tx, x, cancellationToken);
        foreach (var x in batch.Vouchers) await UpsertVoucherAsync(connection, tx, x, cancellationToken);
        foreach (var x in batch.VoucherEntries) await UpsertVoucherEntryAsync(connection, tx, x, cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }

    private static Task UpsertGroupAsync(SqlConnection c, SqlTransaction tx, LedgerGroup g, CancellationToken ct) => ExecuteAsync(c, tx, "MERGE dbo.ledger_groups AS t USING (SELECT @Id AS id) s ON t.group_id=s.id WHEN MATCHED THEN UPDATE SET name=@Name,parent_group_name=@Parent,is_reserved=@Reserved,updated_at_utc=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(group_id,name,parent_group_name,is_reserved) VALUES(@Id,@Name,@Parent,@Reserved);", ct, new("@Id", g.GroupId), new("@Name", g.Name), new("@Parent", (object?)g.ParentGroupName ?? DBNull.Value), new("@Reserved", g.IsReserved));
    private static Task UpsertLedgerAsync(SqlConnection c, SqlTransaction tx, LedgerMaster l, CancellationToken ct) => ExecuteAsync(c, tx, "MERGE dbo.ledgers AS t USING (SELECT @Id AS id) s ON t.ledger_id=s.id WHEN MATCHED THEN UPDATE SET name=@Name,parent_group_name=@Parent,gst_in=@GstIn,pan=@Pan,address_line=@Address,is_bill_wise_on=@BillWise,updated_at_utc=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(ledger_id,name,parent_group_name,gst_in,pan,address_line,is_bill_wise_on) VALUES(@Id,@Name,@Parent,@GstIn,@Pan,@Address,@BillWise);", ct, new("@Id", l.LedgerId), new("@Name", l.Name), new("@Parent", (object?)l.ParentGroupName ?? DBNull.Value), new("@GstIn", (object?)l.GstIn ?? DBNull.Value), new("@Pan", (object?)l.Pan ?? DBNull.Value), new("@Address", (object?)l.Address ?? DBNull.Value), new("@BillWise", l.IsBillWiseOn));
    private static Task UpsertStockGroupAsync(SqlConnection c, SqlTransaction tx, StockGroup s, CancellationToken ct) => ExecuteAsync(c, tx, "MERGE dbo.stock_groups AS t USING (SELECT @Id AS id) src ON t.stock_group_id=src.id WHEN MATCHED THEN UPDATE SET name=@Name,parent_stock_group_name=@Parent,updated_at_utc=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(stock_group_id,name,parent_stock_group_name) VALUES(@Id,@Name,@Parent);", ct, new("@Id", s.StockGroupId), new("@Name", s.Name), new("@Parent", (object?)s.ParentStockGroupName ?? DBNull.Value));
    private static Task UpsertStockItemAsync(SqlConnection c, SqlTransaction tx, StockItem s, CancellationToken ct) => ExecuteAsync(c, tx, "MERGE dbo.stock_items AS t USING (SELECT @Id AS id) src ON t.stock_item_id=src.id WHEN MATCHED THEN UPDATE SET name=@Name,stock_group_name=@GroupName,base_unit=@BaseUnit,opening_balance=@OpeningBalance,opening_rate=@OpeningRate,updated_at_utc=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(stock_item_id,name,stock_group_name,base_unit,opening_balance,opening_rate) VALUES(@Id,@Name,@GroupName,@BaseUnit,@OpeningBalance,@OpeningRate);", ct, new("@Id", s.StockItemId), new("@Name", s.Name), new("@GroupName", (object?)s.StockGroupName ?? DBNull.Value), new("@BaseUnit", (object?)s.BaseUnit ?? DBNull.Value), new("@OpeningBalance", s.OpeningBalance), new("@OpeningRate", s.OpeningRate));
    private static Task UpsertCostCenterAsync(SqlConnection c, SqlTransaction tx, CostCenter cc, CancellationToken ct) => ExecuteAsync(c, tx, "MERGE dbo.cost_centers AS t USING (SELECT @Id AS id) src ON t.cost_center_id=src.id WHEN MATCHED THEN UPDATE SET name=@Name,parent_cost_center_name=@Parent,updated_at_utc=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(cost_center_id,name,parent_cost_center_name) VALUES(@Id,@Name,@Parent);", ct, new("@Id", cc.CostCenterId), new("@Name", cc.Name), new("@Parent", (object?)cc.ParentCostCenterName ?? DBNull.Value));
    private static Task UpsertVoucherAsync(SqlConnection c, SqlTransaction tx, VoucherMaster v, CancellationToken ct) => ExecuteAsync(c, tx, "MERGE dbo.vouchers AS t USING (SELECT @Id AS id) src ON t.voucher_id=src.id WHEN MATCHED THEN UPDATE SET voucher_number=@Num,voucher_date=@Date,voucher_type=@Type,party_ledger_name=@Party,total_amount=@Amount,source_system=@Source,updated_at_utc=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(voucher_id,voucher_number,voucher_date,voucher_type,party_ledger_name,total_amount,source_system) VALUES(@Id,@Num,@Date,@Type,@Party,@Amount,@Source);", ct, new("@Id", v.VoucherId), new("@Num", v.VoucherNumber), new("@Date", v.VoucherDate), new("@Type", v.VoucherType), new("@Party", (object?)v.PartyLedgerName ?? DBNull.Value), new("@Amount", v.TotalAmount), new("@Source", v.Source));
    private static Task UpsertVoucherEntryAsync(SqlConnection c, SqlTransaction tx, VoucherEntry e, CancellationToken ct) => ExecuteAsync(c, tx, "MERGE dbo.voucher_entries AS t USING (SELECT @VoucherId AS voucher_id,@LineNo AS line_no) src ON t.voucher_id=src.voucher_id AND t.line_no=src.line_no WHEN MATCHED THEN UPDATE SET ledger_name=@Ledger,amount=@Amount,is_debit=@IsDebit,cost_center=@Cost,narration=@Narr,updated_at_utc=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(voucher_id,line_no,ledger_name,amount,is_debit,cost_center,narration) VALUES(@VoucherId,@LineNo,@Ledger,@Amount,@IsDebit,@Cost,@Narr);", ct, new("@VoucherId", e.VoucherId), new("@LineNo", e.LineNo), new("@Ledger", e.LedgerName), new("@Amount", e.Amount), new("@IsDebit", e.IsDebit), new("@Cost", (object?)e.CostCenter ?? DBNull.Value), new("@Narr", (object?)e.Narration ?? DBNull.Value));

    private static async Task ExecuteAsync(SqlConnection connection, SqlTransaction tx, string sql, CancellationToken ct, params SqlParameter[] p)
    {
        await using var cmd = new SqlCommand(sql, connection, tx);
        cmd.Parameters.AddRange(p);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
