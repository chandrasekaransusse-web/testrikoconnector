# Tally Connector (Windows EXE + SQL DB)

This connector runs as a Windows service and continuously syncs Tally data.

## Data coverage now

- Ledger groups
- Ledgers (masters)
- Stock groups
- Stock items
- Cost centers
- Voucher headers
- Voucher entries

## Connection modes

- `Mode = TallyHttp` (Tally XML over HTTP)
- `Mode = Odbc` (cloud/bridge ODBC)

## Target modes

- `Target = SqlServer` (default, production)
- `Target = Json` (debug)

## Database included

Use `deploy/schema.sql`. It creates database and tables:

- `ledger_groups`
- `ledgers`
- `stock_groups`
- `stock_items`
- `cost_centers`
- `vouchers`
- `voucher_entries` (FK to `vouchers`)

Run in SQL Server Management Studio:

```sql
:r .\deploy\schema.sql
```

## Build EXE locally on Windows

```powershell
./scripts/publish-win-x64.ps1
```

Output:

- `artifacts/publish/TallyConnectorApp.exe`

Install service:

```powershell
./scripts/install-service.ps1 -ExePath "C:\path\to\artifacts\publish\TallyConnectorApp.exe"
```

## Build EXE from GitHub Actions

Workflow included: `.github/workflows/build-windows.yml`

- Trigger workflow
- Download artifact `tally-connector-win-x64`
- It contains built `TallyConnectorApp.exe`

## Config

Edit `src/TallyConnectorApp/appsettings.json`:

- source mode (`TallyHttp` / `Odbc`)
- source connection details
- SQL target connection string
- sync interval

## Important

I cannot commit a compiled `.exe` from this environment because .NET download/build is blocked by network proxy (HTTP 403). The repository now includes everything required to generate the EXE on Windows or via GitHub Actions and test directly with your Tally instance.
