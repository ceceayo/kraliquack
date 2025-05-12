using Microsoft.FluentUI.AspNetCore.Components;
using UGS.AdminPanel.Components;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UGS.AdminPanel.Components.Pages;
using UGS.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddLogging();

builder.Services.AddProblemDetails();

builder.AddNpgsqlDbContext<UGS.Shared.SharedUniversalGameServerDataBaseContext>("postgresdb");

builder.AddRedisClient("cache");

builder.Services.AddTransient<IDatabase>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase(0));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

builder.Services.AddSingleton<UserAccountService>(new UserAccountService("admin"));
builder.Services.AddTransient<ConfigService>(sp => new ConfigService(sp.GetRequiredService<SharedUniversalGameServerDataBaseContext>()));

builder.Services.AddDataGridEntityFrameworkAdapter();

builder.Services.AddAntiforgery();
builder.Services.AddLogging();
builder.Services.AddHttpLogging();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();
