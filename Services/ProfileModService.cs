using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.View.Windows;
using TCML_Class_library;

namespace TCM_Launcher.Services
{
    public class ProfileModService : IProfileModService
    {
        private readonly IDownloadService downloadService;
        private readonly IBackendService backendService;
        private readonly IAppSettingsService appSettingsService;
        public ProfileModService(IDownloadService downloadService, IBackendService backendService, IAppSettingsService appSettingsService)
        {
            this.downloadService = downloadService;
            this.backendService = backendService;
            this.appSettingsService = appSettingsService;
        }

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
                        var existingMod = manifest.Mods.FirstOrDefault(m => m.Id == currentDetails.Id);

                        if(existingMod != null)
                        {
                            existingMod.Version = currentVersion.VersionNumber;
                            existingMod.FileName = primaryFile.FileName;
                            existingMod.DownloadUrl = primaryFile.Url;
                            existingMod.Name = currentDetails.Title;
                            existingMod.Source = selectedVersion.Source;
                            existingMod.IconUrl = currentDetails.IconUrl;
                            existingMod.Author = currentDetails.Author;
                            existingMod.IsEnabled = true;
                        }
                        else
                        {
                            manifest.Mods.Add(new ProfileModInfo
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
                            });
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

        public async Task ExportModpackAsync(string profileId, string profileName)
        {
            string exportName = string.Join("_", profileName.Split(Path.GetInvalidFileNameChars()));
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Export modpack",
                    FileName = $"{exportName}_modpack",
                    DefaultExt = "json",
                    Filter = "JSON files (*.json)|*.json",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string source = GetManifestPath(profileId);
                    string destination = dialog.FileName;

                    if (File.Exists(source))
                    {
                        File.Copy(source, destination, true);

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"/select,\"{destination}\"",
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show("No mods to export");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when exporting modpack.", ex);
            }
        }

        public string GetManifestPath(string profileId)
            => Path.Combine(Constants.ProfilesPath, profileId, Constants.ProfileModsManifest);

        public async Task<ProfileModInfo?> ImportModAsync(string profileId, string modName, string modVersion, string fileName, string sourceFile, bool clientSide, bool serverSide, bool missingJar = false)
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
                            Client_Side = clientSide ? "required" : "optional",
                            Server_Side = serverSide ? "required" : "optional",
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

        public async Task ImportModpackAsync(string profileId)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Select a modpack to import",
                    Filter = "JSON files (*.json)|*.json",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                };
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string source = dialog.FileName;
                    if (File.Exists(source))
                    {
                        string json = await File.ReadAllTextAsync(source);
                        var manifest = JsonSerializer.Deserialize<ProfileModpackManifest>(json);
                        if (manifest != null)
                        {
                            manifest.ProfileId = profileId;
                            manifest.LastUpdated = DateTime.Now;
                            string destination = Path.Combine(Constants.ProfilesPath, profileId, Constants.ProfileModsManifest);

                            await File.WriteAllTextAsync(destination, json);
                        }
                    }
                }
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
                var missingImportedModNames = missingMods.Where(m => m.Source == ModSource.Imported).ToList();
                if (missingImportedModNames.Count > 0)
                {
                    foreach (var missingMod in missingImportedModNames)
                    {
                        ImportModView i = App.ServiceProvider.GetRequiredService<ImportModView>();
                        i.Owner = App.Current.MainWindow;
                        i.Owner.Opacity = 0.4;
                        i.viewModel.ModName = missingMod.Name;
                        i.viewModel.ModVersion = missingMod.Version;
                        i.viewModel.ClientSide = missingMod.Client_Side == "required";
                        i.viewModel.ServerSide = missingMod.Server_Side == "required";
                        var imported = i.ShowDialog();
                        if(imported == true)
                        {
                            await ImportModAsync(profileId, i.viewModel.ModName, i.viewModel.ModVersion, i.viewModel.FileName, i.viewModel.SourceFile, i.viewModel.ClientSide, i.viewModel.ServerSide, true);
                        }
                        i.Owner.Opacity = 1;
                    }
                }
            }
            catch(Exception ex)
            {

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
                    //if (serverAddress != null)
                    //{
                    //    var req = (string var) => var == "required";
                    //    var serverOnly = mods.Where(m => req(m.Server_Side) && !req(m.Client_Side)).ToList();
                    //    foreach (var m in serverOnly)
                    //    {
                    //        m.IsEnabled = false;
                    //    }
                    //    toggledServerOnly = true;
                    //}
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

                //if(serverAddress != null && !toggledServerOnly)
                //{
                //    await ToggleQuickJoinServerMods(profileId, modsFolder, false, manifest);
                //}
                
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

        //public async Task ToggleQuickJoinServerMods(string profileId, string modsFolder, bool enable, ProfileModpackManifest? manifest = null)
        //{
        //    if (!Directory.Exists(modsFolder)) return;
        //    try
        //    {
        //        manifest ??= await LoadManifestAsync(profileId);
        //        if (manifest != null && manifest.Mods != null)
        //        {
        //            var req = (string var) => var == "required";
        //            var serverOnly = manifest.Mods.Where(m => req(m.Server_Side) && !req(m.Client_Side)).ToList();
        //            await Task.Run(() =>
        //            {
        //                Parallel.ForEach(manifest.Mods, new ParallelOptions { MaxDegreeOfParallelism = 10 }, m =>
        //                {
        //                    string currentName = enable ? m.FileName + ".disabled" : m.FileName;
        //                    string newName = enable ? m.FileName : m.FileName + ".disabled";

        //                    string currentPath = Path.Combine(modsFolder, currentName);
        //                    string newPath = Path.Combine(modsFolder, newName);
        //                    if (File.Exists(currentPath))
        //                    {
        //                        File.Move(currentPath, newPath);
        //                    }
        //                    m.IsEnabled = enable;
        //                });
        //            });
        //            await SaveManifestAsync(profileId, manifest);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("There was an exception when toggleing only server mods", ex);
        //    }
        //}
    }
}
