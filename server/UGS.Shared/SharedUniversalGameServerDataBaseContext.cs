using Microsoft.EntityFrameworkCore;
using UGS.Shared.DbModels;

namespace UGS.Shared
{
    public class SharedUniversalGameServerDataBaseContext(DbContextOptions<SharedUniversalGameServerDataBaseContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ConfigKey> ConfigKeys { get; set; }
        public DbSet<GameSpec> GameSpecs { get; set; }
        public DbSet<World> Worlds { get; set; }
    }
}
