using MigrationWorker;
using UGS.Shared;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<SharedUniversalGameServerDataBaseContext>("postgresdb");

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
