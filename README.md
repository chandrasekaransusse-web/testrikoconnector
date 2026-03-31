# Tally Connector (Windows Sync App)

This repository now includes a starter **Windows background connector** that continuously pulls accounting data from Tally on a schedule.

It supports two data-source modes:

1. **Tally HTTP/XML mode** (for local Tally/TallyPrime exposing XML on port 9000).
2. **ODBC mode** (for cloud setups or environments where Tally is exposed through an ODBC endpoint/bridge).

## Project layout

- `src/TallyConnectorApp/Program.cs` - host bootstrap + Windows service registration.
- `src/TallyConnectorApp/TallySyncWorker.cs` - recurring sync loop.
- `src/TallyConnectorApp/Connectors/TallyHttpConnector.cs` - XML request/response connector.
- `src/TallyConnectorApp/Connectors/OdbcConnector.cs` - ODBC query connector.
- `src/TallyConnectorApp/Connectors/JsonFileSyncTarget.cs` - sample sink that writes synced vouchers to JSON files.
- `src/TallyConnectorApp/appsettings.json` - mode + connection config.

## How it works

1. The worker starts and reads the `Connector` config.
2. Based on `Connector:Mode`, it chooses either `TallyHttp` or `Odbc` connector.
3. It fetches vouchers from source.
4. It writes output to `sync-output/*.json`.
5. It waits for `SyncIntervalSeconds` and repeats forever.

## Configure for your requirement

### Local Tally over HTTP/XML

Set:

```json
"Connector": {
  "Mode": "TallyHttp",
  "TallyHttp": {
    "Url": "http://localhost:9000"
  }
}
```

### Cloud/remote via ODBC

Set:

```json
"Connector": {
  "Mode": "Odbc",
  "Odbc": {
    "ConnectionString": "your-cloud-odbc-connection-string",
    "VoucherQuery": "SELECT TOP 100 VoucherNumber, VoucherDate, Amount, Ledger FROM Vouchers"
  }
}
```

## Run as a Windows service

From a Windows machine with .NET 8 SDK/runtime:

```bash
dotnet publish ./src/TallyConnectorApp/TallyConnectorApp.csproj -c Release -r win-x64 --self-contained false
```

Then install service (PowerShell as Administrator):

```powershell
sc.exe create TallyConnectorSyncService binPath= "C:\path\to\publish\TallyConnectorApp.exe"
sc.exe start TallyConnectorSyncService
```

## Next production steps

- Replace `JsonFileSyncTarget` with your destination (API, SQL Server, PostgreSQL, etc.).
- Add incremental sync checkpoints (last voucher ID / modified timestamp).
- Add authentication + encrypted secrets handling.
- Add health checks and retry/backoff policy.
- Add installer (MSI/Inno Setup) for one-click deployment.
