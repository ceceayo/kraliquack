using Carter;
using RabbitMQ.Client;
using StackExchange.Redis;
using UGS.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddLogging();

builder.Services.AddHttpLogging();

builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddOpenApiDocument();

builder.Services.AddCarter();



builder.AddNpgsqlDbContext<UGS.Shared.SharedUniversalGameServerDataBaseContext>("postgresdb");

builder.AddRedisClient("cache");

builder.Services.AddTransient<IDatabase>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase(0));

builder.Services.AddSingleton<UGS.Shared.UserAccountService>(new UserAccountService("api"));
builder.Services.AddTransient< UGS.Shared.ConfigService > (sp => new ConfigService(sp.GetRequiredService<UGS.Shared.SharedUniversalGameServerDataBaseContext>()));


builder.AddRabbitMQClient("queue");

builder.Services.AddTransient<RabbitMQ.Client.IModel>(sp => sp.GetRequiredService<IConnection>().CreateModel());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseWebSockets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseOpenApi();
    app.UseSwaggerUi();

}



app.MapCarter();

app.MapDefaultEndpoints();

app.Run();

