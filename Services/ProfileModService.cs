using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Windows.Forms;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.ViewModel.UserControls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCM_Launcher.ViewModel.UserControls.Panels;
using TCML_Class_library;

namespace TCM_Launcher.Services
{
    public class ProfileModService : IProfileModService
    {
        private readonly IDownloadService downloadService;
        private readonly IBackendService backendService;
        private readonly IAppSettingsService appSettingsService;
        private readonly IGameProfileService gameProfileService;
        private readonly IProfileSettingsService profileSettingsService;
        private readonly IOverlayService overlayService;
        private readonly IDownloadedModpacksService downloadedModpacksService;
        private readonly Lazy<ILauncherService> launcherService;
        public ProfileModService(IDownloadService downloadService, IBackendService backendService, IAppSettingsService appSettingsService, IDownloadedModpacksService downloadedModpacksService,
            IGameProfileService gameProfileService, IProfileSettingsService profileSettingsService, IOverlayService overlayService, IServiceProvider serviceProvider)
        {
            this.downloadService = downloadService;
            this.appSettingsService = appSettingsService;
            this.gameProfileService = gameProfileService;
            this.profileSettingsService = profileSettingsService;
            this.overlayService = overlayService;
            this.downloadedModpacksService = downloadedModpacksService;

            this.backendService = backendService;
            this.backendService.OnModpackDownloadedFromStorage = ImportModpackDirectlyAsync;
            this.launcherService = new Lazy<ILauncherService>(() => serviceProvider.GetRequiredService<ILauncherService>());
        }
        public event Action<string>? OnUpdatedModpack;

        public async Task<List<ProfileModInfo>> AddModWithDependenciesAsync(string profileId, string mcVersion, ModDetails modDetails, ModVersion selectedVersion)
        {
            try
            {
                var manifest = await LoadManifestAsync(profileId);
                manifest.Mods ??= new List<ProfileModInfo>();

                var processedProjectIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var processingQueue = new Queue<(ModDetails details, ModVersion version)>();
                processingQueue.Enqueue((modDetails, selectedVersion));

                while (processingQueue.Count > 0)
                {
                    var (currentDetails, currentVersion) = processingQueue.Dequeue();

                    if (processedProjectIds.Contains(currentDetails.Id)) continue;

                    processedProjectIds.Add(currentDetails.Id);
                    var primaryFile = currentVersion.Files.FirstOrDefault();
                    if (primaryFile != null)
                    {
                        var index = manifest.Mods.FindIndex(m => m.Id == currentDetails.Id);

                        var mod = new ProfileModInfo
                        {
                            Id = currentDetails.Id,
                            Name = currentDetails.Title,
                            FileName = primaryFile.FileName,
                            DownloadUrl = primaryFile.Url,
                            Source = selectedVersion.Source,
                            Version = currentVersion.VersionNumber,
                            IconUrl = currentDetails.IconUrl,
                            Author = currentDetails.Author,
                            Client_Side = currentDetails.Client_Side,
                            Server_Side = currentDetails.Server_Side,
                        };
                        if (index != -1)
                        {
                            manifest.Mods[index] = mod;
                        }
                        else
                        {
                            manifest.Mods.Add(mod);
                        }
                    }

                    var requiredDependencies = currentVersion.Dependencies?
                        .Where(d => d.DependencyType.Equals("required", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (requiredDependencies == null || !requiredDependencies.Any())
                        continue;

                    foreach (var dep in requiredDependencies)
                    {
                        bool isAlreadyInManifest = manifest.Mods.Any(m => m.Id == dep.ProjectId);
                        if (processedProjectIds.Contains(dep.ProjectId) || isAlreadyInManifest)
                            continue;
                        string? depModrinthId = selectedVersion.Source == ModSource.Modrinth ? dep.ProjectId : null;
                        string? depCurseforgeId = selectedVersion.Source == ModSource.CurseForge ? dep.ProjectId : null;
                        ModDetails? depDetails = await backendService.GetModDetailsAsync(
                            dep.ProjectId, 
                            depModrinthId, 
                            depCurseforgeId, 
                            mcVersion, 
                            selectedVersion.Source,
                            false
                        );

                        if (depDetails != null && depDetails.Versions.Any())
                        {
                            var depVersion = depDetails.Versions.FirstOrDefault();
                            if (depVersion != null)
                            {
                                processingQueue.Enqueue((depDetails, depVersion));
                            }
                        }
                    }
                }

                await SaveManifestAsync(profileId, manifest);
                return manifest.Mods;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception while addin mod to profile", ex);
                return new();
            }
        }

        public async Task<string?> ExportModpackAsync(string profileId, string profileName, bool exportModJARs = true, List<ProfileModInfo>? mods = null, bool exportOnlyImportedFiles = false)
        {
            string exportName = string.Join("_", profileName.Split(Path.GetInvalidFileNameChars()));
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Export TCM modpack",
                    FileName = $"{exportName}.tcmp",
                    DefaultExt = "tcmp",
                    Filter = "TCM Modpack (*.tcmp)|*.tcmp",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    OverwritePrompt = true,
                };
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string dest = dialog.FileName;
                    string profileFolder = Path.Combine(Constants.ProfilesPath, profileId);

                    await Task.Run(async () =>
                    {
                        using var fileStream = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None);
                        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);

                        var manifest = await LoadManifestAsync(profileId);
                        if (manifest != null)
                        {
                            if (mods != null)
                            {
                                manifest.Mods = mods.Where(m => m.IsEnabled).ToList();
                            }
                            var entry = archive.CreateEntry("manifest.json");
                            using var entryStream = entry.Open();
                            await JsonSerializer.SerializeAsync(entryStream, manifest);
                        }

                        if (exportModJARs && mods != null)
                        {
                            string modsFolder = Path.Combine(Constants.ProfilesPath, profileId, "mods");
                            if (Directory.Exists(modsFolder))
                            {
                                foreach (var mod in mods)
                                {
                                    if (!mod.IsEnabled) continue;

                                    bool isManualOrNoUrl = mod.Source == ModSource.Imported || string.IsNullOrEmpty(mod.DownloadUrl);
                                    bool shouldExport = !exportOnlyImportedFiles || isManualOrNoUrl;

                                    if (!shouldExport) continue;

                                    string activePath = Path.Combine(modsFolder, mod.FileName);
                                    string disabledPath = Path.Combine(modsFolder, mod.FileName + ".disabled");

                                    string? actualFilePath = File.Exists(activePath) ? activePath
                                                           : File.Exists(disabledPath) ? disabledPath
                                                           : null;
                                    if (actualFilePath != null)
                                    {
                                        string entryName = $"overrides/mods/{Path.GetFileName(actualFilePath)}";
                                        archive.CreateEntryFromFile(actualFilePath, entryName);
                                    }
                                }
                            }
                        }
                    });
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{dest}\"",
                        UseShellExecute = true,
                    });
                    return dest;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when exporting modpack.", ex);
                return null;
            }
        }

        public string GetManifestPath(string profileId)
            => Path.Combine(Constants.ProfilesPath, profileId, Constants.ProfileManifest);

        /// <summary>
        /// Imports a mod from a specified source.
        /// </summary>
        /// <param name="profileId">Id of the profile.</param>
        /// <param name="modName">Name of the mod.</param>
        /// <param name="modVersion">Version of the mod.</param>
        /// <param name="fileName">Safe filename of the .jar file.</param>
        /// <param name="sourceFile">Absolute path of the .jar file.</param>
        /// <param name="clientSide">Client side requirement of the mod.</param>
        /// <param name="serverSide">Server side requirement of the mod.</param>
        /// <param name="missingJar">Indicates the import method: Full import or only missing jar</param>
        /// <returns>The imported mod's info.</returns>
        public async Task<ProfileModInfo?> ImportModAsync(string profileId, string modName, string modVersion, string fileName, string sourceFile, string clientSide, string serverSide, bool missingJar = false)
        {
            try
            {
                var manifest = await LoadManifestAsync(profileId);
                if (manifest != null && manifest.Mods != null)
                {
                    var existingMod = manifest.Mods.FirstOrDefault(m => m.FileName == fileName);
                    if (existingMod != null && !missingJar)
                    {
                        MessageBox.Show("This mod is already imported.");
                        return null;
                    }
                    string destFolder = Path.Combine(Constants.ProfilesPath, profileId, "mods");
                    if(!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);
                    string destFile = Path.Combine(destFolder, fileName);

                    await Task.Run(() => File.Copy(sourceFile, destFile, true));

                    if (!missingJar)
                    {
                        var importedMod = new ProfileModInfo
                        {
                            Id = fileName,
                            Name = modName,
                            Version = modVersion,
                            FileName = fileName,
                            Client_Side = clientSide,
                            Server_Side = serverSide,
                            Source = ModSource.Imported,
                        };
                        manifest.Mods.Add(importedMod);
                        await SaveManifestAsync(profileId, manifest);
                        MessageBox.Show($"{modName} was imported successfully");
                        return importedMod;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when importing mod.", ex);
                return null;
            }
        }

        public async Task ImportModpackAsync(string profileId, string? source = null)
        {
            try
            {
                if (string.IsNullOrEmpty(source))
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = "Select a TCM Modpack to import",
                        Filter = "TCM Modpack (*.tcmp)|*.tcmp",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    };

                    if (dialog.ShowDialog() != DialogResult.OK) return;
                    source = dialog.FileName;
                }

                string profileFolder = Path.Combine(Constants.ProfilesPath, profileId);
                string modsFolder = Path.Combine(profileFolder, "mods");

                if (!Directory.Exists(profileFolder)) Directory.CreateDirectory(profileFolder);
                if (!Directory.Exists(modsFolder)) Directory.CreateDirectory(modsFolder);

                await Task.Run(async () =>
                {
                    using var archive = ZipFile.OpenRead(source);

                    foreach (var entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name)) continue;

                        if (entry.FullName.Equals("manifest.json", StringComparison.OrdinalIgnoreCase))
                        {
                            using var stream = entry.Open();
                            var manifest = await JsonSerializer.DeserializeAsync<ProfileModpackManifest>(stream);
                            if (manifest != null)
                            {
                                manifest.LastUpdated = DateTime.Now;
                                string destination = Path.Combine(profileFolder, Constants.ProfileManifest);
                                await File.WriteAllTextAsync(destination, JsonSerializer.Serialize(manifest));
                            }
                        }
                        else if (entry.FullName.StartsWith("overrides/mods/", StringComparison.OrdinalIgnoreCase))
                        {
                            string targetFile = Path.Combine(modsFolder, entry.Name);
                            entry.ExtractToFile(targetFile, overwrite: true);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception while importing modpack.", ex);
            }
        }

        private async Task CheckMissingImportedFiles(string profileId, List<ProfileModInfo> mods, HashSet<string>? existingFiles, HashSet<string>? expectedFiles)
        {
            try
            {
                var missingMods = mods.Where(m =>
                {
                    string expectedName = m.IsEnabled ? m.FileName : m.FileName + ".disabled";
                    return !existingFiles.Contains(expectedName);
                }).ToList();
                var missingImportedModNames = missingMods.Where(m => m.Source == ModSource.Imported || string.IsNullOrWhiteSpace(m.DownloadUrl)).ToList();
                if (missingImportedModNames.Count > 0)
                {
                    foreach (var missingMod in missingImportedModNames)
                    {
                        var details = await overlayService.ShowModImportPanel(missingMod);
                        if (details != null)
                        {
                            string sourceFile = details.Value.filePath;
                            var modInfo = details.Value.modInfo;
                            var res = await ImportModAsync(profileId, modInfo.Name, modInfo.Version, modInfo.FileName, sourceFile, modInfo.Client_Side, modInfo.Server_Side, true);

                            if (res != null) existingFiles?.Add(missingMod.FileName);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Error("There was an exception when importing missing files.", ex);
            }
        }

        public async Task<bool> IsVersionBindedToProfileAsync(string profileId, ModVersion version)
        {
            try
            {
                var manifest = await LoadManifestAsync(profileId);
                if (manifest.Mods == null) return false;
                string primaryFileName = version.Files.FirstOrDefault()?.FileName ?? "";

                var existingVersion = manifest.Mods.FirstOrDefault(m => m.Id == version.ProjectId ||
                (!string.IsNullOrEmpty(primaryFileName) && m.FileName == primaryFileName));
                return existingVersion != null && existingVersion.Version == version.VersionNumber;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exceptoin checking if version is adde to profile.", ex);
                return false;
            }
        }

        public async Task CreateProfileManifest(string profileId, string profileName, string mcVersion, string forgeVersion, string modpackId)
        {
            string destPath = Path.Combine(Constants.ProfilesPath, profileId, Constants.ProfileManifest);
            try
            {
                ProfileModpackManifest manifest = new ProfileModpackManifest
                {
                    ProfileName = profileName,
                    Dependencies = new ModpackDependency
                    {
                        MinecraftVersion = mcVersion,
                        ForgeVersion = forgeVersion
                    },
                    ModpackId = modpackId
                };

                string json = JsonSerializer.Serialize(manifest);
                await File.WriteAllTextAsync(destPath, json);
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when creating profile manifest.", ex);
            }
        }

        public async Task<ProfileModpackManifest> LoadManifestAsync(string profileId)
        {
            string path = GetManifestPath(profileId);

            if (!File.Exists(path)) return new ProfileModpackManifest();

            string json = await File.ReadAllTextAsync(path);
            
            return JsonSerializer.Deserialize<ProfileModpackManifest>(json) 
                ?? new ProfileModpackManifest();
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

        public async Task<bool> SyncProfileModsAsync(string profileId, IProgress<double>? progress = null, IProgress<string>? status = null, string? serverAddress = null)
        {
            string modsFolder = Path.Combine(Constants.ProfilesPath, profileId, "mods");
            if (!Directory.Exists(modsFolder)) Directory.CreateDirectory(modsFolder);
            try
            {
                var manifest = await LoadManifestAsync(profileId);
                var existingFiles = Directory.GetFiles(modsFolder).Select(Path.GetFileName).ToHashSet();
                var mods = manifest.Mods ?? new List<ProfileModInfo>();
                var expectedFiles = mods.Select(m => m.IsEnabled ? m.FileName : m.FileName + ".disabled").ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>();
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = appSettingsService.AppSettings.MaximumParalellDownloads };
                var missingMods = mods.Where(m =>
                {
                    string expectedName = m.IsEnabled ? m.FileName : m.FileName + ".disabled";
                    return !existingFiles.Contains(expectedName);
                }).ToList();
                await CheckMissingImportedFiles(profileId, mods, existingFiles, expectedFiles);
                bool toggledServerOnly = false;
                int missing = missingMods.Count;
                if (missing == 0) progress?.Report(100);
                else
                {
                    int downloaded = 0;
                    status?.Report("Installing mods");
                    await Parallel.ForEachAsync(missingMods, parallelOptions, async (mod, token) =>
                    {
                        if (mod.Source != ModSource.Imported)
                        {
                            
                            string targetName = mod.IsEnabled ? mod.FileName : mod.FileName + ".disabled";
                            await downloadService.DownloadFileAsync(mod.DownloadUrl, modsFolder, targetName);
                            int currentCompleted = Interlocked.Increment(ref downloaded);
                            double currentPercent = (double)currentCompleted / missing * 100;
                            progress?.Report(currentPercent);
                        }
                    });
                }
                
                var orphanMods = existingFiles.Where(file => !expectedFiles.Contains(file)).ToList();
                foreach (var orphanMod in orphanMods)
                {
                    string fullPath = Path.Combine(modsFolder, orphanMod);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception during the synchronization of the mods in profile", ex);
                return false;
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

        public async Task<GameProfile?> ImportModpackDirectlyAsync(string filePath, bool importFromStorage = false, FirestoreModpack? modpack = null, bool update = false, GameProfile? profileToUpdate = null)
        {
            bool? res = false;
            if (!importFromStorage)
            {
                res = await overlayService.ShowPopupPanelAsync("Import profile", "You are trying to import a profile. Would you like to continue?", PopupAction.IMPORT);
            }
            if (res == false && !importFromStorage) return null;
            try
            {
                bool fileExists = File.Exists(filePath);
                if (!fileExists)
                {
                    Constants.MessageBoxError("The source file doesn't exist.");
                    return null;
                }

                ProfileModpackManifest? manifest = null;

                using (var archive = ZipFile.OpenRead(filePath))
                {
                    var manifestEntry = archive.GetEntry("manifest.json");
                    if (manifestEntry != null)
                    {
                        using var stream = manifestEntry.Open();
                        manifest = await JsonSerializer.DeserializeAsync<ProfileModpackManifest>(stream);
                    }
                }

                if (manifest == null)
                {
                    MessageBox.Show("Invalid modpack.");
                    return null;
                }
                GameProfile? p = null;
                if (!update)
                {
                    p = await gameProfileService.AddProfileAsync(manifest.ProfileName, manifest.Dependencies.MinecraftVersion, manifest.Dependencies.ForgeVersion, manifest.ModpackId);
                    if (p != null)
                    {
                        var settings = await profileSettingsService.SetProfileSettingsAsync(new ProfileSettings
                        {
                            GameProfileId = p.Id,
                            Ram = Constants.DefaultRam
                        });
                    }
                    else return null;
                }
                else p = await gameProfileService.GetProfileAsync(profileToUpdate.Id);
                await ImportModpackAsync(p.Id, filePath);
                if (p != null && !update)
                {
                    var profilesView = App.ServiceProvider.GetRequiredService<ProfilesViewModel>();
                    var installVm = App.ServiceProvider.GetRequiredService<ProfileInstallIndicatorViewModel>();

                    var progress = new Progress<double>(progress => installVm.Progress = progress);
                    var status = new Progress<string>(status => installVm.Status = status);
                    var visible = new Progress<bool>(visible => installVm.Visible = visible);

                    installVm.ProfileName = p.ProfileName;
                    installVm.ProfileId = p.Id;
                    installVm.OnRemoveRequested = () =>
                    {
                        var existing = profilesView.Installs.FirstOrDefault(vm => vm.ProfileId == installVm.ProfileId);
                        if (existing != null) profilesView.Installs.Remove(existing);
                    };
                    profilesView.Installs.Add(installVm);

                    var fileName = await launcherService.Value.CreateProfileAsync(p.Id, p.MCVersion, p.ForgeVersion, progress, status, visible);
                    if (fileName != null)
                    {
                        p.Installed = true;
                        p.FileName = fileName;
                        await gameProfileService.UpdateProfileAsync(p);
                        var vm = App.ServiceProvider.GetRequiredService<ProfileDetailsViewModel>();
                        vm.Profile = p;
                        profilesView.Profiles.Add(vm);
                    }
                }
                if (importFromStorage)
                {
                    if (modpack != null)
                    {
                        if (!update)
                        {
                            await downloadedModpacksService.AddModpackAsync(modpack);
                            await gameProfileService.UpdateProfileAsync(new GameProfile { Id = p.Id, ModpackId = modpack.Id });
                        }
                        else
                        {
                            await gameProfileService.UpdateProfileAsync(new GameProfile { Id = p.Id, PackReleaseNumber = modpack.ReleaseNumber });
                            OnUpdatedModpack?.Invoke(modpack.Id);
                        }
                    }
                    var sourceDir = Path.GetDirectoryName(filePath);
                    if(Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
                }
                return p;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when importing modpack directly.", ex);
                return null;
            }
        }

        public async Task UpdateModAsync(string profileId, ProfileModInfo mod)
        {
            try
            {
                var manifest = await LoadManifestAsync(profileId);
                if (manifest != null)
                {
                    var existing = manifest.Mods.FirstOrDefault(m => m.Id == mod.Id);
                    if (existing != null)
                    {
                        existing.Name = mod.Name;
                        existing.Version = mod.Version;
                        existing.Client_Side = mod.Client_Side;
                        existing.Server_Side = mod.Server_Side;
                        existing.FileName = mod.FileName;
                        await SaveManifestAsync(profileId, manifest);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when updating mod in manifest", ex);
            }
        }
    }
}
