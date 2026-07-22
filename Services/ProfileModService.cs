using System.IO;
using System.Text.Json;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.Mods;
using TCML_Class_library;

namespace TCM_Launcher.Services
{
    public class ProfileModService : IProfileModService
    {
        private readonly IDownloadService downloadService;
        private readonly IBackendService backendService;
        public ProfileModService(IDownloadService downloadService, IBackendService backendService)
        {
            this.downloadService = downloadService;
            this.backendService = backendService;
        }

        public async Task AddModWithDependenciesAsync(string profileId, string mcVersion, ModDetails modDetails, ModVersion selectedVersion)
        {
            try
            {
                var manifest = await LoadManifestAsync(profileId);
                manifest.Mods ??= new List<ProfileModInfo>();

                var processedProjectIds = manifest.Mods.Select(m => m.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

                var processingQueue = new Queue<(ModDetails details, ModVersion version)>();
                processingQueue.Enqueue((modDetails, selectedVersion));

                while (processingQueue.Count > 0)
                {
                    var (currentDetails, currentVersion) = processingQueue.Dequeue();

                    if (!manifest.Mods.Any(m => m.Id == currentDetails.Id))
                    {
                        var primaryFile = currentVersion.Files.FirstOrDefault();
                        if (primaryFile != null)
                        {
                            manifest.Mods.Add(new ProfileModInfo
                            {
                                Id = currentDetails.Id,
                                Name = currentDetails.Title,
                                FileName = primaryFile.FileName,
                                DownloadUrl = primaryFile.Url,
                                Source = currentDetails.Source,
                                Version = currentVersion.VersionNumber,
                                IconUrl = currentDetails.IconUrl,
                                Author = currentDetails.Author,
                                IsEnabled = true
                            });
                        }
                    }

                    processedProjectIds.Add(currentDetails.Id);

                    var requiredDependencies = currentVersion.Dependencies?
                        .Where(d => d.DependencyType.Equals("required", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (requiredDependencies == null || !requiredDependencies.Any())
                        continue;

                    foreach (var dep in requiredDependencies)
                    {
                        if (processedProjectIds.Contains(dep.ProjectId))
                            continue;

                        ModDetails? depDetails = await backendService.GetModDetailsAsync(dep.ProjectId, mcVersion, currentDetails.Source, false);

                        if (depDetails != null && depDetails.Versions.Any())
                        {
                            var depVersion = depDetails.Versions.FirstOrDefault();
                            if (depVersion != null)
                            {
                                processedProjectIds.Add(depDetails.Id);
                                processingQueue.Enqueue((depDetails, depVersion));
                            }
                        }
                    }
                }

                await SaveManifestAsync(profileId, manifest);
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception while addin mod to profile", ex);
            }
        }
        public string GetManifestPath(string profileId)
            => Path.Combine(Constants.ProfilesPath, profileId, Constants.ProfileModsManifest);

        public async Task<bool> IsVersionBindedToProfileAsync(string profileId, ModVersion version)
        {
            try
            {
                var manifest = await LoadManifestAsync(profileId);
                var existingVersion = manifest.Mods.FirstOrDefault(m => m.Id == version.ProjectId);
                return existingVersion != null && existingVersion.Version == version.VersionNumber;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exceptoin checking if version is adde to profile.", ex);
                return false;
            }
        }

        public async Task<ProfileModpackManifest> LoadManifestAsync(string profileId)
        {
            string path = GetManifestPath(profileId);

            if (!File.Exists(path)) return new ProfileModpackManifest { ProfileId = profileId };

            string json = await File.ReadAllTextAsync(path);
            
            return JsonSerializer.Deserialize<ProfileModpackManifest>(json) 
                ?? new ProfileModpackManifest { ProfileId = profileId };
        }

        public async Task<string?> RemoveModFromProfile(string profileId, string projectId)
        {
            var manifest = await LoadManifestAsync(profileId);
            if(manifest == null || manifest.Mods == null) return null;
            var modToRemove = manifest.Mods.FirstOrDefault(m => m.Id == projectId);
            if (modToRemove != null)
            {
                manifest.Mods.Remove(modToRemove);
            }
            await SaveManifestAsync(profileId, manifest);
            return projectId;
        }

        public async Task SaveManifestAsync(string profileId, ProfileModpackManifest manifest)
        {
            string path = GetManifestPath(profileId);
            manifest.LastUpdated = DateTime.Now;
            string json = JsonSerializer.Serialize<ProfileModpackManifest>(manifest);
            await File.WriteAllTextAsync(path, json);
        }

        public async Task SyncProfileModsAsync(string profileId)
        {
            string modsFolder = Path.Combine(Constants.ProfilesPath, profileId, "mods");
            try
            {
                var manifest = await LoadManifestAsync(profileId);
                var existingFiles = Directory.GetFiles(modsFolder).Select(Path.GetFileName).ToHashSet();
                var expectedFiles = manifest.Mods.Select(m => m.IsEnabled ? m.FileName : m.FileName + ".disabled").ToHashSet(StringComparer.OrdinalIgnoreCase);
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };
                var missingMods = manifest.Mods.Where(m =>
                {
                    string expectedName = m.IsEnabled ? m.FileName : m.FileName + ".disabled";
                    return !existingFiles.Contains(expectedName);
                }).ToList();
                await Parallel.ForEachAsync(missingMods, parallelOptions, async (mod, token) =>
                {
                    string targetName = mod.IsEnabled ? mod.FileName : mod.FileName + ".disabled";
                    await downloadService.DownloadFileAsync(mod.DownloadUrl, modsFolder, targetName);
                });
                var orphanMods = existingFiles.Where(file => !expectedFiles.Contains(file)).ToList();
                foreach (var orphanMod in orphanMods)
                {
                    string fullPath = Path.Combine(modsFolder, orphanMod);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception during the synchronization of the mods in profile");
            }

        }

        public async Task ToggleModAsync(string profileId, ProfileModInfo mod, bool enable)
        {
            try
            {
                string modsFolder = Path.Combine(Constants.ProfilesPath, profileId, "mods");

                string currentName = enable ? mod.FileName + ".disabled" : mod.FileName;
                string newName = enable ? mod.FileName : mod.FileName + ".disabled";

                string currentPath = Path.Combine(modsFolder, currentName);
                string newPath = Path.Combine(modsFolder, newName);

                if (File.Exists(currentPath))
                {
                    File.Move(currentPath, newPath);
                }

                var manifest = await LoadManifestAsync(profileId);
                var targetMod = manifest.Mods.FirstOrDefault(m => m.Id == mod.Id);
                if (targetMod != null)
                {
                    targetMod.IsEnabled = enable;
                    await SaveManifestAsync(profileId, manifest);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when setting mod IsEnabled property.", ex);
            }
        }
    }
}
