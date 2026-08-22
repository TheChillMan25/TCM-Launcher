using Microsoft.EntityFrameworkCore;
using System.IO;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.DB.Versions;

namespace TCM_Launcher.Core.DBContext
{
    public class LauncherDBContext: DbContext
    {
        public DbSet<GameProfile> GameProfiles { get; set; }
        public DbSet<VanillaVersion> VanillaVersions { get; set; }
        public DbSet<ForgeVersion> ForgeVersions { get; set; }
        public DbSet<ProfileSettings> ProfileSettings { get; set; }
        public DbSet<Server> SavedServers { get; set; }
        public DbSet<AppMetaData> AppMetaData { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string launcherFolder = Constants.LauncherFolder;

            Directory.CreateDirectory(launcherFolder);
            
            string dbPath = Path.Combine(launcherFolder, "launcher.db");
            
            optionsBuilder.UseSqlite($@"Data Source={dbPath}");
        }
    }
}
