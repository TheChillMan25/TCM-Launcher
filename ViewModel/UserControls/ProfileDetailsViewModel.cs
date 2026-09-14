using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Core.Utils.Converters;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class ProfileDetailsViewModel : ViewModelBase
    {
        private readonly IGameProfileService gameProfileService;
        private readonly IProfileModService profileModService;
        private readonly ILauncherService launcherService;
        private readonly IOverlayService overlayService;
        private readonly IFirebaseService firebaseService;
        private readonly IBackendService backendService;
        public ProfileDetailsViewModel(IGameProfileService gameProfileService, ILauncherService launcherService, IProfileModService profileModService,
            IBackendService backendService, IOverlayService overlayService, IFirebaseService firebaseService)
        {
            this.gameProfileService = gameProfileService;
            this.launcherService = launcherService;
            this.profileModService = profileModService;
            this.overlayService = overlayService;
            this.firebaseService = firebaseService;
            this.backendService = backendService;
        }

        public Action<string>? OnDeleteRequested { get; set; }
        public Func<string, string, string, Task>? ShowContentBorwserRequested { get; set; }
        public Action<string, bool>? OnLaunch { get; set; }

        private GameProfile profile;
        public GameProfile Profile
        {
            get { return profile; }
            set
            {
                profile = value;
                OnPropertyChange();
                if(profile != null)
                {
                    HeaderText = $"Forge {profile?.MCVersion}";
                    IsEnable = (profile.Installed == true && !profile.IsPlaying);
                    PlayButtonText = profile.IsPlaying ? "Running" : "Play";
                }
            }
        }


        private List<ProfileModViewModel> allProfileMods = new();
        private ObservableCollection<ProfileModViewModel> modsToShow = new();
        public ObservableCollection<ProfileModViewModel> ModsToShow
        {
            get { return modsToShow; }
            set { modsToShow = value; OnPropertyChange(); }
        }

        private string headerText;
        public string HeaderText
        {
            get { return headerText; }
            set
            {
                headerText = value;
                OnPropertyChange();
            }
        }

        private string playtime;

        public string Playtime
        {
            get { return playtime; }
            set { playtime = value; OnPropertyChange(); }
        }


        private double progressNumber;
        public double ProgressNumber
        {
            get { return progressNumber; }
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    progressNumber = 0;
                }
                else
                {
                    progressNumber = value;
                }
                OnPropertyChange();
            }
        }

        private string progressText;
        public string ProgressText
        {
            get { return progressText; }
            set
            {
                progressText = value;
                OnPropertyChange();
            }
        }

        private bool isEnable;
        public bool IsEnable
        {
            get { return isEnable; }
            set 
            { 
                isEnable = value;
                OnPropertyChange();
            }
        }

        private bool progressVisible;
        public bool ProgressVisible
        {
            get { return progressVisible; }
            set 
            { 
                progressVisible = value;
                OnPropertyChange();
            }
        }

        private string playButtonText = "Play";
        public string PlayButtonText
        {
            get { return playButtonText; }
            set 
            { 
                playButtonText = value;
                OnPropertyChange();
            }
        }

        private string modsCountText;
        public string ModsCountText
        {
            get { return modsCountText; }
            set { modsCountText = value; OnPropertyChange(); }
        }

        private List<string> modSortFilters = new List<string> { "All", "Client only", "Server only", "Exclude client only", "Exclude server only"};
        public List<string> ModSortFilters
        {
            get { return modSortFilters; }
            set { modSortFilters = value; }
        }

        private string selectedFilter = "All";
        public string SelectedFilter
        {
            get { return selectedFilter; }
            set { selectedFilter = value; OnPropertyChange(); SortMods(); }
        }

        private string modTextFilter;

        public string ModTextFilter
        {
            get { return modTextFilter; }
            set 
            { 
                modTextFilter = value;
                if (!string.IsNullOrWhiteSpace(value)) ModsToShow = new(FilterModsByText(modTextFilter, allProfileMods));
                else ModsToShow = new(allProfileMods);
            }
        }

        public void Unsub()
        {
            profileModService.OnUpdatedModpack -= OnModpackUpdatedAsync;
        }

        private async void OnModpackUpdatedAsync(string modpackId)
        {
            if (Profile != null && Profile.ModpackId == modpackId)
            {
                var modpack = firebaseService.CachedModpacks.FirstOrDefault(x => x.Id == modpackId);
                if (modpack != null) Profile.PackReleaseNumber = modpack.ReleaseNumber;

                await LoadProfileModsAsync();
            }
        }

        private List<ProfileModViewModel> FilterModsByText(string filter, List<ProfileModViewModel> source)
        {
            filter = filter.ToLowerInvariant();
            var list = source.Where(m => m.Mod.Name.ToLowerInvariant().Contains(filter)).ToList();
            return list;
        }

        public void SortMods()
        {
            var isReq = (string var) => var == "required";
            var list = allProfileMods;
            switch (SelectedFilter)
            {
                case "Client only":
                    list = allProfileMods.Where(m => isReq(m.Mod.Client_Side) && !isReq(m.Mod.Server_Side)).ToList();
                    if (!string.IsNullOrWhiteSpace(ModTextFilter)) ModsToShow = new(FilterModsByText(ModTextFilter, list));
                    else ModsToShow = new(list);
                    break;
                case "Server only":
                    list = allProfileMods.Where(m => !isReq(m.Mod.Client_Side) && isReq(m.Mod.Server_Side)).ToList();
                    if (!string.IsNullOrWhiteSpace(ModTextFilter)) ModsToShow = new(FilterModsByText(ModTextFilter, list));
                    else ModsToShow = new(list);
                    break;
                case "Exclude client only":
                    list = allProfileMods.Where(m => isReq(m.Mod.Server_Side)).ToList();
                    break;
                case "Exclude server only":
                    list = allProfileMods.Where(m => isReq(m.Mod.Client_Side)).ToList();
                    if (!string.IsNullOrWhiteSpace(ModTextFilter)) ModsToShow = new(FilterModsByText(ModTextFilter, list));
                    else ModsToShow = new(list);
                    break;
                default:
                    if (!string.IsNullOrWhiteSpace(ModTextFilter)) ModsToShow = new(FilterModsByText(ModTextFilter, list));
                    else ModsToShow = new(list);
                    break;
            }
        }

        public async Task LoadProfileModsAsync()
        {
            ModsCountText = "Loading mods...";
            var manifest = await profileModService.LoadManifestAsync(Profile.Id);
            if (manifest != null && manifest.Mods != null)
            {
                UpdateProfileMods(manifest.Mods);
            }
            else
            {
                allProfileMods = new();
                ModsToShow = new();
                ModsCountText = "0 mods";
            }
        }

        private async Task RemoveModFromProfile(string projectId)
        {
            var modId = await profileModService.RemoveModFromProfile(Profile.Id, projectId);
            var mv = allProfileMods.FirstOrDefault(m => m.Mod.Id == modId);
            if (mv != null)
            {
                allProfileMods.Remove(mv);
                ModsToShow.Remove(mv);
                ModsCountText = $"{allProfileMods.Count} mods";
            }
        }

        public async Task OpenProfileSettings()
        {
            if (Profile == null)
            {
                MessageBox.Show("Select a profile.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await overlayService.ShowProfileSettingsAsync(Profile);
        }

        public async Task DeleteProfileAsync()
        {
            try
            {
                IsEnable = false;
                bool success = await gameProfileService.DeleteProfileAsync(Profile.Id);
                if (success)
                {
                    string profileDir = Path.Combine(Constants.ProfilesPath, Profile.Id);
                    if (Directory.Exists(profileDir)) Directory.Delete(profileDir, true);
                    OnDeleteRequested?.Invoke(Profile.Id);
                }
                else
                {
                    IsEnable = true;
                }
            }
            catch (IOException ex)
            {
                Logger.Error("There was an exception during deleting profile", ex);
                MessageBox.Show("Couldn't delete the profile because it is still running or the files are still in use. Close the game before deleting.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception during deleting profile", ex);
                MessageBox.Show("An error occured during deleting profile.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public void OpenProfileFolder()
        {
            gameProfileService.OpenProfileFolder(Profile.Id);
        }

        public async Task StartGameAsync(string serverAddress = null)
        {
            var profileToLaunch = Profile;
            if (profileToLaunch == null) return;
            try
            {
                await CheckForModpackUpdateAsync();
                profileToLaunch.IsPlaying = true;
                if (Profile?.Id == profileToLaunch.Id)
                {
                    IsEnable = profileToLaunch.Installed == true && profileToLaunch.IsPlaying == false;
                    PlayButtonText = "Running";
                }
                OnLaunch?.Invoke(Profile.Id, true);

                var progress = new Progress<double>(percent => ProgressNumber = percent);
                var status = new Progress<string>(status => ProgressText = status);
                var progressVisible = new Progress<bool>(progress => ProgressVisible = progress);

                var startTime = DateTime.Now;

                await launcherService.LaunchProfileAsync(Profile, progress, status, progressVisible, serverAddress);

                var dif = (DateTime.Now - startTime).TotalSeconds;
                Profile.PlayTime += dif;
                Playtime = PlaytimeConverter.ConvertToString(Profile.PlayTime ?? 0d);
                await gameProfileService.UpdateProfileAsync(new GameProfile { Id = Profile.Id, PlayTime = Profile.PlayTime });
            }
            finally
            {
                OnLaunch?.Invoke(Profile.Id, false);
                profileToLaunch.IsPlaying = false;
                if (Profile?.Id == profileToLaunch.Id)
                {
                    IsEnable = profileToLaunch.Installed == true && profileToLaunch.IsPlaying == false;
                    PlayButtonText = "Play";
                }
            }
        }

        public void ShowContentBorwser()
        {
            ShowContentBorwserRequested?.Invoke(Profile.Id, Profile.ProfileName, Profile.MCVersion);
        }

        public async Task ExportModpackAsync()
        {
            var modpackId = await overlayService.ShowExportModpackPanelAsync(Profile, allProfileMods);
            if (modpackId != null) Profile.ModpackId = modpackId;
        }
        public async Task ImportModpackAsync()
        {
            await profileModService.ImportModpackAsync(Profile.Id);
            await LoadProfileModsAsync();
        }
        public async Task LoadProfileAsync(GameProfile p)
        {
            if (Profile == null || Profile.Id != p.Id) 
            {
                Profile = p;
                Playtime = PlaytimeConverter.ConvertToString(Profile.PlayTime ?? 0d);

                profileModService.OnUpdatedModpack -= OnModpackUpdatedAsync; 
                profileModService.OnUpdatedModpack += OnModpackUpdatedAsync;

                await CheckForModpackUpdateAsync();

                _ = LoadProfileModsAsync();
            }
        }

        private async Task CheckForModpackUpdateAsync()
        {
            var modpack = firebaseService.CachedModpacks.FirstOrDefault(x => x.Id == Profile.ModpackId);
            if (modpack != null)
            {
                if (Profile.PackReleaseNumber < modpack.ReleaseNumber)
                {
                    var res = await overlayService.ShowPopupPanelAsync(
                        "Update available",
                        $"There is an update available for \"{modpack.Name}\" modpack (by {modpack.OwnerName})",
                        Panels.PopupAction.UPDATE);
                    if (res == true)
                    {
                        await backendService.DownloadModpackAsync(modpack, true, Profile);
                    }
                }
            }
        }

        public async Task OpenModSettings(ProfileModViewModel model)
        {
            var result = await overlayService.ShowModSettingsPanel(model.Mod);
            if (result != null)
            {
                var existing = ModsToShow.FirstOrDefault(m => m.Mod.Id == model.Mod.Id);
                if (existing != null)
                {
                    existing.Mod.Name = result.Name;
                    existing.Mod.Version = result.Version;
                    existing.Mod.Client_Side = result.Client_Side;
                    existing.Mod.Server_Side = result.Server_Side;
                    CollectionViewSource.GetDefaultView(ModsToShow)?.Refresh();
                    await profileModService.UpdateModAsync(Profile.Id, result);
                }
            }
        }

        /// <summary>
        /// Opens the mod details window. After saving, imports the mod. Depending on the usecase it only copies the .jar file or imports a whole new mod into the profile.
        /// </summary>
        /// <returns></returns>
        public async Task ImportModAsync()
        {
            var result = await overlayService.ShowModImportPanel();
            if (result != null)
            {
                string sourcePath = result.Value.filePath;
                var mod = result.Value.modInfo;
                var mInfo = await profileModService.ImportModAsync(Profile.Id, mod.Name, mod.Version, mod.FileName, sourcePath, mod.Client_Side, mod.Server_Side);

                if (mInfo != null)
                {
                    var modVm = App.ServiceProvider.GetRequiredService<ProfileModViewModel>();
                    modVm.Mod = mInfo;
                    modVm.OnModRemoveRequested = RemoveModFromProfile;
                    modVm.OnModSettingsRequested = OpenModSettings;
                    modVm.OnToggleModRequested = ToggleMod;
                    modVm.ModEnabled = mInfo.IsEnabled;
                    modVm.MissingJar = false;
                    ModsToShow.Add(modVm);
                }
            }
        }

        private async Task ToggleMod(ProfileModInfo mod, bool enable)
        {
            try
            {
                await profileModService.ToggleModAsync(Profile.Id, mod, enable);
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when toggling mod.", ex);
            }
        }

        public void UpdateProfileMods(List<ProfileModInfo> mods)
        {
            string modsFolder = Path.Combine(Constants.ProfilesPath, Profile.Id, "mods");
            var existingFiles = Directory.Exists(modsFolder) ?
                Directory.GetFiles(modsFolder).Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string?>(StringComparer.OrdinalIgnoreCase);
            var existingVmMap = ModsToShow.ToDictionary(vm => vm.Mod.Id);
            var allUpdatedViewModels = mods.AsParallel().Select(modInfo =>
            {
                if (existingVmMap.TryGetValue(modInfo.Id, out var existingVm))
                {
                    existingVm.Mod = modInfo;
                    return existingVm;
                }
                else
                {
                    var vm = App.ServiceProvider.GetRequiredService<ProfileModViewModel>();
                    vm.Mod = modInfo;
                    bool missingJar = !existingFiles.Contains(modInfo.FileName);
                    vm.MissingJar = missingJar && vm.Mod.Source == ModSource.Imported;
                    vm.OnModRemoveRequested = RemoveModFromProfile;
                    vm.OnModSettingsRequested = OpenModSettings;
                    vm.OnToggleModRequested = ToggleMod;
                    vm.ModEnabled = modInfo.IsEnabled;
                    return vm;
                }
            }).OrderBy(vm => vm.Mod.Name).ToList();
            allProfileMods = allUpdatedViewModels;
            ModsToShow = new(allUpdatedViewModels);
            ModsCountText = $"{allProfileMods.Count} mods";
        }
    }
}
