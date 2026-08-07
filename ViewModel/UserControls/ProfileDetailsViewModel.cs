using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
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
        public ProfileDetailsViewModel(IGameProfileService gameProfileService, ILauncherService launcherService, IProfileModService profileModService)
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


        private ObservableCollection<ProfileModViewModel> allProfileMods = new();
        private ObservableCollection<ProfileModViewModel> profileMods = new();
        public ObservableCollection<ProfileModViewModel> ProfileMods
        {
            get { return profileMods; }
            set { profileMods = value; OnPropertyChange(); }
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

        public void SortMods()
        {
            var req = (string var) => var == "required"; 
            switch (SelectedFilter)
            {
                case "Client only":
                    ProfileMods = new(allProfileMods.Where(m => req(m.Mod.Client_Side) && !req(m.Mod.Server_Side)).ToList());
                    break;
                case "Server only":
                    ProfileMods = new(allProfileMods.Where(m => !req(m.Mod.Client_Side) && req(m.Mod.Server_Side)).ToList());
                    break;
                case "Exclude client only":
                    ProfileMods = new(allProfileMods.Where(m => !req(m.Mod.Client_Side)).ToList());
                    break;
                case "Exclude server only":
                    ProfileMods = new(allProfileMods.Where(m => req(m.Mod.Client_Side)).ToList());
                    break;
                default:
                    ProfileMods = allProfileMods; 
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
                ProfileMods = new();
            }
        }

        private async Task RemoveModFromProfile(string projectId)
        {
            var modId = await profileModService.RemoveModFromProfile(Profile.Id, projectId);
            var mv = ProfileMods.FirstOrDefault(m => m.Mod.Id == modId);
            if (mv != null) ProfileMods.Remove(mv);
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
            await profileModService.ExportModpackAsync(Profile.Id, Profile.ProfileName);
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
        public async Task ImportModAsync(bool missingJar = false, ProfileModViewModel model = null)
        {
            ImportModView i = App.ServiceProvider.GetRequiredService<ImportModView>();
            i.Owner = App.Current.MainWindow;
            i.Owner.Opacity = 0.4;
            var iVm = i.viewModel;
            if(model != null)
            {
                iVm.ModName = model.Mod.Name;
                iVm.ModVersion = model.Mod.Version;
                iVm.ClientSide = model.Mod.Client_Side == "required";
                iVm.ServerSide = model.Mod.Server_Side == "required";
            }
            var imported = i.ShowDialog();
            if (imported == true)
            {
                var mInfo = await profileModService.ImportModAsync(Profile.Id, iVm.ModName, iVm.ModVersion, iVm.FileName, iVm.SourceFile, iVm.ClientSide, iVm.ServerSide, missingJar);
                if(mInfo != null)
                {
                    var modVm = App.ServiceProvider.GetRequiredService<ProfileModViewModel>();
                    modVm.Mod = mInfo;
                    modVm.OnModRemoveRequested = RemoveModFromProfile;
                    modVm.OnImportFileRequested = ImportModAsync;
                    modVm.MissingJar = false;
                    ProfileMods.Add(modVm);
                }
                else
                {
                    var existing = ProfileMods.FirstOrDefault(m => m.Mod.Id == iVm.FileName);
                    if (existing != null) existing.MissingJar = false;
                }
            }
            i.Owner.Opacity = 1;
        }

        public void UpdateProfileMods(List<ProfileModInfo> mods)
        {
            string modsFolder = Path.Combine(Constants.ProfilesPath, Profile.Id, "mods");
            var existingFiles = Directory.Exists(modsFolder) ?
                Directory.GetFiles(modsFolder).Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string?>(StringComparer.OrdinalIgnoreCase);
            var existingVmMap = ProfileMods.ToDictionary(vm => vm.Mod.Id);
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
                    vm.OnImportFileRequested = ImportModAsync;
                    return vm;
                }
            }).OrderBy(vm => vm.Mod.Name).ToList();
            allProfileMods = new(allUpdatedViewModels);
            ProfileMods = new(allUpdatedViewModels);
            ModsCountText = $"{ProfileMods.Count} mods";
        }
    }
}
