using RabbitMQ.Client;
using StackExchange.Redis;
using UGS.Shared;
using UGS.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<SharedUniversalGameServerDataBaseContext>("postgresdb");

builder.AddRedisClient("cache");


builder.Services.AddSingleton<IDatabase>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase(0));

builder.Services.AddSingleton<UGS.Shared.UserAccountService>(new UserAccountService("api"));
builder.Services.AddSingleton<UGS.Shared.ConfigService>(sp => new ConfigService(sp.GetRequiredService<SharedUniversalGameServerDataBaseContext>()));

builder.AddRabbitMQClient("queue");

builder.Services.AddSingleton<RabbitMQ.Client.IModel>(sp => sp.GetRequiredService<IConnection>().CreateModel());

builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();
