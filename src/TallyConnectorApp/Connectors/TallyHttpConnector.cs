using System.Globalization;
using System.Text;
using System.Xml.Linq;
using TallyConnectorApp.Configuration;
using TallyConnectorApp.Models;

namespace TallyConnectorApp.Connectors;

public sealed class TallyHttpConnector(TallyHttpOptions options) : IDataSourceConnector
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds))
    };

    public async Task<IReadOnlyCollection<VoucherRecord>> FetchVouchersAsync(CancellationToken cancellationToken)
    {
        var payload = BuildEnvelope();
        using var request = new HttpRequestMessage(HttpMethod.Post, options.Url)
        {
            Content = new StringContent(payload, Encoding.UTF8, "text/xml")
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseResponse(xml);
    }

    private string BuildEnvelope()
    {
        var companyTag = string.IsNullOrWhiteSpace(options.CompanyName)
            ? string.Empty
            : $"<SVCurrentCompany>{System.Security.SecurityElement.Escape(options.CompanyName)}</SVCurrentCompany>";

        return $$"""
        <ENVELOPE>
          <HEADER>
            <VERSION>1</VERSION>
            <TALLYREQUEST>Export</TALLYREQUEST>
            <TYPE>Data</TYPE>
            <ID>Voucher Register</ID>
          </HEADER>
          <BODY>
            <DESC>
              <STATICVARIABLES>
                {{companyTag}}
              </STATICVARIABLES>
              <TDL>
                <TDLMESSAGE>
                  <COLLECTION NAME="CodexVoucherCollection" ISMODIFY="No">
                    <TYPE>Voucher</TYPE>
                    <FETCH>DATE, VOUCHERNUMBER, AMOUNT, LEDGERNAME</FETCH>
                  </COLLECTION>
                </TDLMESSAGE>
              </TDL>
            </DESC>
          </BODY>
        </ENVELOPE>
        """;
    }

    private static IReadOnlyCollection<VoucherRecord> ParseResponse(string xml)
    {
        var document = XDocument.Parse(xml);
        var vouchers = new List<VoucherRecord>();

        foreach (var node in document.Descendants("VOUCHER"))
        {
            var number = node.Element("VOUCHERNUMBER")?.Value ?? "UNKNOWN";
            var ledger = node.Element("LEDGERNAME")?.Value ?? "N/A";
            var amountText = node.Element("AMOUNT")?.Value ?? "0";
            var dateText = node.Element("DATE")?.Value ?? DateTime.UtcNow.ToString("yyyyMMdd");

            _ = decimal.TryParse(amountText, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount);
            _ = DateTime.TryParseExact(
                dateText,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var voucherDate);

            vouchers.Add(new VoucherRecord(number, voucherDate, amount, ledger, "TallyHttp"));
        }

        return vouchers;
    }
}
