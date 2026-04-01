using System.Globalization;
using System.Text;
using System.Xml.Linq;
using TallyConnectorApp.Configuration;
using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public sealed class TallyHttpConnector(TallyHttpOptions options) : IDataSourceConnector
{
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds)) };

    public async Task<SyncBatch> FetchAsync(CancellationToken cancellationToken)
    {
        var xml = await SendRequestAsync(BuildEnvelope(), cancellationToken);
        return ParseResponse(xml);
    }

    private async Task<string> SendRequestAsync(string payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, options.Url) { Content = new StringContent(payload, Encoding.UTF8, "text/xml") };
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private string BuildEnvelope()
    {
        var companyTag = string.IsNullOrWhiteSpace(options.CompanyName) ? string.Empty : $"<SVCurrentCompany>{System.Security.SecurityElement.Escape(options.CompanyName)}</SVCurrentCompany>";
        return $$"""
        <ENVELOPE><HEADER><VERSION>1</VERSION><TALLYREQUEST>Export</TALLYREQUEST><TYPE>Data</TYPE><ID>All Masters And Vouchers</ID></HEADER><BODY><DESC><STATICVARIABLES>{{companyTag}}</STATICVARIABLES><TDL><TDLMESSAGE>
        <COLLECTION NAME="CodexLedgerGroups" ISMODIFY="No"><TYPE>Group</TYPE><FETCH>GUID,NAME,PARENT,RESERVEDNAME</FETCH></COLLECTION>
        <COLLECTION NAME="CodexLedgers" ISMODIFY="No"><TYPE>Ledger</TYPE><FETCH>GUID,NAME,PARENT,GSTREGISTRATIONNUMBER,INCOMETAXNUMBER,ADDRESS,ISBILLWISEON</FETCH></COLLECTION>
        <COLLECTION NAME="CodexStockGroups" ISMODIFY="No"><TYPE>StockGroup</TYPE><FETCH>GUID,NAME,PARENT</FETCH></COLLECTION>
        <COLLECTION NAME="CodexStockItems" ISMODIFY="No"><TYPE>StockItem</TYPE><FETCH>GUID,NAME,PARENT,BASEUNITS,OPENINGBALANCE,OPENINGRATE</FETCH></COLLECTION>
        <COLLECTION NAME="CodexCostCenters" ISMODIFY="No"><TYPE>CostCentre</TYPE><FETCH>GUID,NAME,PARENT</FETCH></COLLECTION>
        <COLLECTION NAME="CodexVouchers" ISMODIFY="No"><TYPE>Voucher</TYPE><FETCH>MASTERID,VOUCHERNUMBER,DATE,VOUCHERTYPENAME,PARTYLEDGERNAME,AMOUNT,NARRATION,ALLLEDGERENTRIES.LIST</FETCH></COLLECTION>
        </TDLMESSAGE></TDL></DESC></BODY></ENVELOPE>
        """;
    }

    private static SyncBatch ParseResponse(string xml)
    {
        var doc = XDocument.Parse(xml);
        var groups = doc.Descendants("GROUP").Select(x => new LedgerGroup(Text(x, "GUID") ?? Guid.NewGuid().ToString("N"), Text(x, "NAME") ?? "UNKNOWN", Text(x, "PARENT"), !string.IsNullOrWhiteSpace(Text(x, "RESERVEDNAME")))).ToList();
        var ledgers = doc.Descendants("LEDGER").Select(x => new LedgerMaster(Text(x, "GUID") ?? Guid.NewGuid().ToString("N"), Text(x, "NAME") ?? "UNKNOWN", Text(x, "PARENT"), Text(x, "GSTREGISTRATIONNUMBER"), Text(x, "INCOMETAXNUMBER"), Text(x, "ADDRESS"), string.Equals(Text(x, "ISBILLWISEON"), "Yes", StringComparison.OrdinalIgnoreCase))).ToList();
        var stockGroups = doc.Descendants("STOCKGROUP").Select(x => new StockGroup(Text(x, "GUID") ?? Guid.NewGuid().ToString("N"), Text(x, "NAME") ?? "UNKNOWN", Text(x, "PARENT"))).ToList();
        var stockItems = doc.Descendants("STOCKITEM").Select(x => new StockItem(Text(x, "GUID") ?? Guid.NewGuid().ToString("N"), Text(x, "NAME") ?? "UNKNOWN", Text(x, "PARENT"), Text(x, "BASEUNITS"), ParseDecimal(Text(x, "OPENINGBALANCE")), ParseDecimal(Text(x, "OPENINGRATE")))).ToList();
        var costCenters = doc.Descendants("COSTCENTRE").Select(x => new CostCenter(Text(x, "GUID") ?? Guid.NewGuid().ToString("N"), Text(x, "NAME") ?? "UNKNOWN", Text(x, "PARENT"))).ToList();

        var vouchers = new List<VoucherMaster>();
        var entries = new List<VoucherEntry>();
        foreach (var v in doc.Descendants("VOUCHER"))
        {
            var voucherId = Text(v, "MASTERID") ?? Guid.NewGuid().ToString("N");
            vouchers.Add(new VoucherMaster(voucherId, Text(v, "VOUCHERNUMBER") ?? "UNKNOWN", ParseDate(Text(v, "DATE")), Text(v, "VOUCHERTYPENAME") ?? "UNKNOWN", Text(v, "PARTYLEDGERNAME"), ParseDecimal(Text(v, "AMOUNT")), "TallyHttp"));
            var line = 1;
            foreach (var e in v.Descendants("ALLLEDGERENTRIES.LIST"))
                entries.Add(new VoucherEntry(voucherId, line++, Text(e, "LEDGERNAME") ?? "UNKNOWN", ParseDecimal(Text(e, "AMOUNT")), string.Equals(Text(e, "ISDEEMEDPOSITIVE"), "No", StringComparison.OrdinalIgnoreCase), Text(e, "COSTCENTRENAME"), Text(v, "NARRATION")));
        }

        return new SyncBatch { LedgerGroups = groups, Ledgers = ledgers, StockGroups = stockGroups, StockItems = stockItems, CostCenters = costCenters, Vouchers = vouchers, VoucherEntries = entries, SyncedAtUtc = DateTime.UtcNow };
    }

    private static string? Text(XElement element, string name) => element.Element(name)?.Value?.Trim();
    private static DateTime ParseDate(string? value) => DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt) ? dt : DateTime.UtcNow;
    private static decimal ParseDecimal(string? value) => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m;
}
