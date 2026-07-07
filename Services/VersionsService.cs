using System;
using System.Collections.Generic;
using System.Text;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Model.DB.Versions;

namespace TCM_Launcher.Services
{
    public class VersionsService
    {
        public static VersionsService Instance { get; set; } = new VersionsService();

        public List<VanillaVersion> GetVanillaVersions()
        {
            try
            {
                using var db = new LauncherDBContext();
                return db.VanillaVersions.OrderByDescending(v => v.VersionName).ToList();
            }
            catch
            {
                return new List<VanillaVersion>();
            }
        }

        public List<ForgeVersion> GetForgeVersions(string mcVersion)
        {
            try
            {
                using var db = new LauncherDBContext();
                var versions = db.ForgeVersions.Where(v => v.MCVersion == mcVersion).ToList();
                var sorted = versions.OrderByDescending(v =>
                {
                    if (Version.TryParse(v.VersionName, out Version parsedVersion))
                    {
                        return parsedVersion;
                    }
                    return new Version(0, 0, 0);
                }).ToList();
                return sorted;
            }
            catch
            {
                return new List<ForgeVersion>();
            }
        }
    }
}
