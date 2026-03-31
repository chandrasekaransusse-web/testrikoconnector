using System.Data.Odbc;
using TallyConnectorApp.Configuration;
using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public sealed class OdbcConnector(OdbcOptions options) : IDataSourceConnector
{
    public async Task<IReadOnlyCollection<VoucherRecord>> FetchVouchersAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("ODBC connection string is required when mode is set to Odbc.");
        }

        var results = new List<VoucherRecord>();

        await using var connection = new OdbcConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = options.VoucherQuery;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var number = reader["VoucherNumber"]?.ToString() ?? "UNKNOWN";
            var date = reader["VoucherDate"] is DateTime dt ? dt : DateTime.UtcNow;
            var amount = decimal.TryParse(reader["Amount"]?.ToString(), out var parsedAmount) ? parsedAmount : 0m;
            var ledger = reader["Ledger"]?.ToString() ?? "N/A";

            results.Add(new VoucherRecord(number, date, amount, ledger, "ODBC"));
        }

        return results;
    }
}
