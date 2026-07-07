using CmlLib.Core;
using CmlLib.Core.Installer.Forge;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using TCM_Launcher.Core;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.DB.Versions;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;
using TCM_Launcher.View;
using TCM_Launcher.View.Windows;

namespace TCM_Launcher.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            LoadProfiles();
            OpenLastPlayedProfile();
            CheckVersionsAsync();
        }

        private ObservableCollection<GameProfile> gameProfiles;

        public ObservableCollection<GameProfile> GameProfiles
        {
            get { return gameProfiles; }
            set
            {
                gameProfiles = value;
                OnPropertyChange();
            }
        }

        private GameProfile selectedGameProfile;

        public GameProfile SelectedGameProfile
        {
            get { return selectedGameProfile; }
            set
            {
                selectedGameProfile = value;
                OnPropertyChange();
                OnPropertyChange(nameof(HasSelectedProfile));
                HeaderText = $"Forge {selectedGameProfile?.MCVersion}";
            }
        }
        public bool HasSelectedProfile => SelectedGameProfile != null;
        private string headerText;

        public string HeaderText
        {
            get { return headerText; }
            set {
                headerText = value;
                OnPropertyChange();
            }
        }




        public async Task ShowNewProfileWindow()
        {
            NewProfileView npw = new NewProfileView();
            npw.Owner = Application.Current.MainWindow;
            Application.Current.MainWindow.Opacity = 0.4;
            bool created = npw.ShowDialog() ?? false;
            if (created) 
            {
                bool success = await CreateProfile(npw.NewProfileData);

                if (success) MessageBox.Show("Installation complete");
            }
            Application.Current.MainWindow.Opacity = 1;
        }

        public void OpenProfileSettings()
        {
            if(SelectedGameProfile == null)
            {
                MessageBox.Show("Select a profile.");
            }
            ProfileSettingsView ps = new ProfileSettingsView(SelectedGameProfile);
            ps.Owner = Application.Current.MainWindow;
            Application.Current.MainWindow.Opacity = 0.4;
            ps.ShowDialog();            
            Application.Current.MainWindow.Opacity = 1;
        }

        public void LoadProfiles()
        {
            var profilesList = GameProfileService.Instance.GetAllGameProfiles();
            GameProfiles = new ObservableCollection<GameProfile>(profilesList);
        }

        private async Task<bool> CreateProfile(GameProfile data)
        {
            try
            {
                var p = GameProfileService.Instance.AddProfile(data.ProfileName, data.MCVersion, data.ForgeVersion);
                ProfileSettingsService.Instance.SetProfileSettings(new ProfileSettings
                {
                    GameProfileId = p.Id,
                    Ram = Constants.DefaultRam,
                });
                var fileName = await LauncherService.Instance.CreateProfileAsync(p.Id, data.MCVersion, data.ForgeVersion);
                if (!string.IsNullOrEmpty(fileName))
                {
                    GameProfileService.Instance.UpdateProfile(p.Id, new GameProfile {
                        ProfileName = p.ProfileName,
                        MCVersion = p.MCVersion,
                        ForgeVersion = p.ForgeVersion,
                        FileName = fileName,
                        Installed = true 
                    });
                    LoadProfiles();
                    SelectedGameProfile = GameProfiles.FirstOrDefault(prof => prof.Id == p.Id);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured during profile creation; {ex.Message}");
                return false;
            }
        }

        private void OpenLastPlayedProfile()
        {
            if (GameProfiles.Count == 0) return;
            try
            {
                var p = GameProfileService.Instance.GetLastPlayedProfile();
                if (p == null)
                {
                    return;
                }
                SelectedGameProfile = p;
            }
            catch
            {
                return;
            }
        }

        public void DeleteProfile()
        {
            try
            {
                bool success = GameProfileService.Instance.DeleteProfile(SelectedGameProfile.Id);
                string profileDir = Path.Combine(Constants.ProfilesPath, SelectedGameProfile.Id);
                if (success)
                {
                    if(Directory.Exists(profileDir))  Directory.Delete(profileDir, true);
                    GameProfiles.Remove(SelectedGameProfile);

                    if (GameProfiles.Count > 0)
                    {
                        SelectedGameProfile = GameProfiles[0];
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occured during deleting profile.");
            }
            
        }

        private async Task CheckVersionsAsync()
        {
            string sharedPath = Path.Combine(Constants.AppDataPath, "Shared");
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
