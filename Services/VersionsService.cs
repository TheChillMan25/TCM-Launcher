using CmlLib.Core;
using CmlLib.Core.Installer.Forge;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.IO;
using System.Windows;
using TCM_Launcher.Core;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Model.DB.Versions;

namespace TCM_Launcher.Services
{
    public class VersionsService
    {
        public static VersionsService Instance { get; set; } = new VersionsService();
        public Task SyncTask { get; private set; }

        public void StartVersionCheck()
        {
            if (SyncTask == null || SyncTask.IsCompleted)
            {
                SyncTask = CheckVersionsAsync();
            }
        }

        public async Task<List<VanillaVersion>> GetVanillaVersions()
        {
            try
            {
                using var db = new LauncherDBContext();
                return await db.VanillaVersions.OrderByDescending(v => v.VersionName).ToListAsync();
            }
            catch
            {
                return new List<VanillaVersion>();
            }
        }

        public async Task<List<ForgeVersion>> GetForgeVersions(string mcVersion)
        {
            try
            {
                using var db = new LauncherDBContext();
                var versions = await db.ForgeVersions.Where(v => v.MCVersion == mcVersion).ToListAsync();
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

        private async Task CheckVersionsAsync()
        {
            string sharedPath = Path.Combine(Constants.LauncherFolder, "Shared");
            MinecraftPath dummyPath = new MinecraftPath(sharedPath);
            var dummyLauncher = new MinecraftLauncher(dummyPath);
            var dummyForgeInstaller = new ForgeInstaller(dummyLauncher);

            try
            {
                var vVersions = await dummyLauncher.GetAllVersionsAsync();

                var nV = (string v, uint sV) => {
                    var vSplit = v.Split(".");
                    if (vSplit.Length >= 2 && uint.TryParse(vSplit[0], out var mainV) && uint.TryParse(vSplit[1], out var secV))
                    {
                        return mainV > 1 || (mainV == 1 && secV >= sV);
                    }
                    return false;
                };

                var releaseVersions = vVersions.Where(v => v.Type == "release").Where(v => nV(v.Name, 14)).ToList();

                List<VanillaVersion> vanillaEntities = releaseVersions.Select(v => new VanillaVersion
                {
                    VersionName = v.Name,
                    Type = v.Type,
                }).ToList();

                var forgeEntitiesBag = new ConcurrentBag<ForgeVersion>();
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };

                await Parallel.ForEachAsync(releaseVersions, parallelOptions, async (v, token) =>
                {
                    try
                    {
                        var fVersions = await dummyForgeInstaller.GetForgeVersions(v.Name);

                        if (fVersions != null)
                        {
                            foreach (var fV in fVersions)
                            {
                                forgeEntitiesBag.Add(new ForgeVersion
                                {
                                    VersionName = fV.ForgeVersionName,
                                    MCVersion = v.Name,
                                    Recommended = fV.IsRecommendedVersion
                                });
                            }
                        }
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {

                    }
                });

                await CheckVersionsAsync(vanillaEntities);
                await CheckVersionsAsync(forgeEntitiesBag.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured during fetching versions: {ex.Message}");
            }
        }

        private async Task CheckVersionsAsync(List<VanillaVersion> freshData)
        {
            try
            {
                using var db = new LauncherDBContext();

                var existingVersions = db.VanillaVersions.Select(v => v.VersionName).ToHashSet();

                var newVersions = freshData.Where(v => !existingVersions.Contains(v.VersionName)).ToList();

                if (newVersions.Any())
                {
                    db.VanillaVersions.AddRange(newVersions);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show($"Error during saving vanilla versions to database: {ex.Message}");
            }
        }

        private async Task CheckVersionsAsync(List<ForgeVersion> freshData)
        {
            try
            {
                using var db = new LauncherDBContext();

                var existingForge = db.ForgeVersions.Select(f => f.VersionName).ToHashSet();

                var newForge = freshData.Where(f => !existingForge.Contains(f.VersionName)).ToList();

                if (newForge.Any())
                {
                    db.ForgeVersions.AddRange(newForge);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show($"Error during saving forge versions to database: {ex.Message}");
            }
        }
    }
}
