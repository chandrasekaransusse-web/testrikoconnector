param(
  [string]$ExePath = "C:\TallyConnector\TallyConnectorApp.exe",
  [string]$ServiceName = "TallyConnectorSyncService"
)

if (-not (Test-Path $ExePath)) {
  throw "EXE not found at $ExePath"
}

sc.exe stop $ServiceName | Out-Null
sc.exe delete $ServiceName | Out-Null

sc.exe create $ServiceName binPath= "\"$ExePath\"" start= auto
sc.exe start $ServiceName

Write-Host "Service $ServiceName installed and started."
