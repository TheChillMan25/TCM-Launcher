using Microsoft.EntityFrameworkCore;
using System.IO;
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(Constants.AppDataPath, "launcher.db");

            optionsBuilder.UseSqlite($@"Data Source={dbPath}");
        }
    }
}
