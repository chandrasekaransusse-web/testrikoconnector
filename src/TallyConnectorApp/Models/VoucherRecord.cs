namespace TallyConnectorApp.Models;

public sealed record VoucherRecord(
    string VoucherNumber,
    DateTime VoucherDate,
    decimal Amount,
    string Ledger,
    string Source);
