using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM;
using TCM_Launcher.View.Windows;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class ProfileDetailsViewModel : ViewModelBase
    {
        private readonly IGameProfileService gameProfileService;
        private readonly IProfileModService profileModService;
        private readonly ILauncherService launcherService;
        public ProfileDetailsViewModel(IGameProfileService gameProfileService, ILauncherService launcherService, IProfileModService profileModService, IBackendService backendService   )
        {
            this.gameProfileService = gameProfileService;
            this.launcherService = launcherService;
            this.profileModService = profileModService;
        }
        public Action<string>? OnDeleteRequested { get; set; }
        public Action<GameProfile>? OnPinRequested { get; set; }
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

        private string pinText;
        public string PinText
        {
            get { return pinText; }
            set { pinText = value; OnPropertyChange(); }
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
                    list = new(allProfileMods.Where(m => isReq(m.Mod.Client_Side)).ToList());
                    if (!string.IsNullOrWhiteSpace(ModTextFilter)) ModsToShow = new(FilterModsByText(ModTextFilter, list));
                    else ModsToShow = new(list);
                    break;
                default:
                    if (!string.IsNullOrWhiteSpace(ModTextFilter)) ModsToShow = new(FilterModsByText(ModTextFilter, list));
                    else ModsToShow = new(list);
                    break;
            }
        }

        public Action<string, string>? OnProfileNameUpdated { get; set; }
        public Func<GameProfile, Task> OnProfileLoaded { get; private set; }

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
                ModsCountText = "No mods";
            }
        }

        private async Task RemoveModFromProfile(string projectId)
        {
            var modId = await profileModService.RemoveModFromProfile(Profile.Id, projectId);
            var mv = ModsToShow.FirstOrDefault(m => m.Mod.Id == modId);
            if (mv != null) ModsToShow.Remove(mv);
        }

        public async Task OpenProfileSettings()
        {
            if (Profile == null)
            {
                MessageBox.Show("Select a profile.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var ps = App.ServiceProvider.GetRequiredService<ProfileSettingsView>();
            await ps.Initialize(Profile);
            ps.Owner = Application.Current.MainWindow;
            Application.Current.MainWindow.Opacity = 0.4;
            string pName = Profile.ProfileName;
            ps.ShowDialog();
            if(Profile.ProfileName != pName)
                OnProfileNameUpdated?.Invoke(Profile.Id, Profile.ProfileName);
            Application.Current.MainWindow.Opacity = 1;
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
                await launcherService.LaunchProfileAsync(Profile, progress, status, progressVisible, serverAddress);
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

        public async Task PinProfile()
        {
            var pinned = Profile.Pinned != null ? !Profile.Pinned : true;
            var s = await gameProfileService.UpdateProfileAsync(Profile.Id, new GameProfile { Pinned = pinned });
            if (s != null) Profile = s;
            PinText = pinned == true ? "Unpin profile" : "Pin profile";
            if (s != null) OnPinRequested?.Invoke(Profile);
        }

        public async Task ExportModpackAsync()
        {
            var e = App.ServiceProvider.GetRequiredService<ExportModpackView>();
            e.Owner = App.Current.MainWindow;
            e.Owner.Opacity = 0.4;
            e.Initialize(Profile.Id, Profile.ProfileName, allProfileMods);
            e.ShowDialog();
            e.Owner.Opacity = 1;
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
                PinText = Profile.Pinned == true ? "Unpin profile" : "Pin profile";
                _ = LoadProfileModsAsync();
            }
        }
        public async Task OpenModSettings(ProfileModViewModel model)
        {
            ModDetailsView i = App.ServiceProvider.GetRequiredService<ModDetailsView>();
            i.Owner = App.Current.MainWindow;
            i.Owner.Opacity = 0.4;

            var iVm = i.viewModel;
            iVm.Initialize(model.Mod, false);
            
            var saved = i.ShowDialog();
            if (saved == true)
            {
                var existing = ModsToShow.FirstOrDefault(m => m.Mod.Id == model.Mod.Id);
                if (existing != null)
                {
                    existing.Mod.Name = iVm.ModName;
                    existing.Mod.Version = iVm.ModVersion;
                    existing.Mod.Client_Side = iVm.ClientSide;
                    existing.Mod.Server_Side = iVm.ServerSide;
                    CollectionViewSource.GetDefaultView(ModsToShow)?.Refresh();
                    await profileModService.UpdateModAsync(Profile.Id, model.Mod);
                }
            }
            i.Owner.Opacity = 1;
        }

        /// <summary>
        /// Opens the mod details window. After saving, imports the mod. Depending on the usecase it only copies the .jar file or imports a whole new mod into the profile.
        /// </summary>
        /// <returns></returns>
        public async Task ImportModAsync()
        {
            ModDetailsView i = App.ServiceProvider.GetRequiredService<ModDetailsView>();
            i.Owner = App.Current.MainWindow;
            i.Owner.Opacity = 0.4;
            var iVm = i.viewModel;

            var saved = i.ShowDialog();
            if (saved == true)
            {
                var mInfo = await profileModService.ImportModAsync(Profile.Id, iVm.ModName, iVm.ModVersion, iVm.FileName, iVm.SourceFile, iVm.ClientSide, iVm.ServerSide);

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
                else
                {
                    var existing = ModsToShow.FirstOrDefault(m => m.Mod.Id == iVm.FileName);
                    if (existing != null) existing.MissingJar = false;
                }
            }
            i.Owner.Opacity = 1; 
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
            allProfileMods = new(allUpdatedViewModels);
            ModsToShow = new(allUpdatedViewModels);
            ModsCountText = $"{ModsToShow.Count} mods";
        }
    }
}
