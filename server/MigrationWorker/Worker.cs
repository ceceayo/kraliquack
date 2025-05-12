using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UGS.Shared;

namespace MigrationWorker;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting migration");
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SharedUniversalGameServerDataBaseContext>();

        logger.LogDebug("Acquired db. Now migrating!");

        await dbContext.Database.MigrateAsync();

        logger.LogInformation("Migration complete.");
    
        if (dbContext.Users.Where(b => b.IsAdmin == true).Count() == 0)
        {
            logger.LogWarning("No admin user! I'll be making one for you!");
            UserAccountService uas = new("admin");
            string username = "root";
            string password = "toor";
            logger.LogWarning($"Username: {username}\nPassword: {password}");
            uas.NewUser(dbContext, logger, username, password, true);
        }

        hostApplicationLifetime.StopApplication();
    }

}