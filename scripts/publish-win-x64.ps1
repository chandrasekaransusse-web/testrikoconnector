param(
  [string]$Configuration = "Release",
  [string]$Runtime = "win-x64",
  [string]$Output = "./artifacts/publish"
)

dotnet publish ./src/TallyConnectorApp/TallyConnectorApp.csproj `
  -c $Configuration `
  -r $Runtime `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  -o $Output

Write-Host "Published EXE at $Output/TallyConnectorApp.exe"
