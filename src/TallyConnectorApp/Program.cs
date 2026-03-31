using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TallyConnectorApp;
using TallyConnectorApp.Configuration;
using TallyConnectorApp.Connectors;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ConnectorOptions>(
    builder.Configuration.GetSection(ConnectorOptions.SectionName));

builder.Services.AddSingleton<IDataSourceConnectorFactory, DataSourceConnectorFactory>();
builder.Services.AddSingleton<IDataSyncTarget, JsonFileSyncTarget>();
builder.Services.AddHostedService<TallySyncWorker>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "TallyConnectorSyncService";
});

await builder.Build().RunAsync();
