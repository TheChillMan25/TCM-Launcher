using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.View.Windows;
using static TCM_Launcher.ViewModel.MainWindowViewModel;

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
        public Action? OnDeleteRequested { get; set; }
        public Func<ContentToShow, Task>? ShowContentBorwserRequested { get; set; }
        public Action<string>? OnLaunch { get; set; }

        private GameProfile selectedGameProfile;
        public GameProfile SelectedGameProfile
        {
            get { return selectedGameProfile; }
            set
            {
                selectedGameProfile = value;
                OnPropertyChange();
                if(selectedGameProfile != null)
                {
                    HeaderText = $"Forge {selectedGameProfile?.MCVersion}";
                    IsEnable = (selectedGameProfile.Installed == true && !selectedGameProfile.IsPlaying);
                    PlayButtonText = selectedGameProfile.IsPlaying ? "Running" : "Play";
                }
            }
        }

        private ObservableCollection<ProfileModViewModel> profileMods;
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

        public async Task LoadProfileModsAsync()
        {
            var manifest = await profileModService.LoadManifestAsync(SelectedGameProfile.Id);
            if (manifest != null && manifest.Mods != null)
            {
                var mods = manifest.Mods.Select(m =>
                {
                    var vm = App.ServiceProvider.GetRequiredService<ProfileModViewModel>();
                    vm.Mod = m;
                    vm.OnModRemoveRequested = RemoveModFromProfile;
                    return vm;
                }).ToList();
                ProfileMods = new ObservableCollection<ProfileModViewModel>(mods);
            }
            else ProfileMods = new ObservableCollection<ProfileModViewModel>();
        }

        private async Task RemoveModFromProfile(string projectId)
        {
            var modId = await profileModService.RemoveModFromProfile(SelectedGameProfile.Id, projectId);
            var mv = ProfileMods.FirstOrDefault(m => m.Mod.Id == modId);
            if (mv != null) ProfileMods.Remove(mv);
        }

        public async Task OpenProfileSettings()
        {
            if (SelectedGameProfile == null)
            {
                MessageBox.Show("Select a profile.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var ps = App.ServiceProvider.GetRequiredService<ProfileSettingsView>();
            await ps.Initialize(SelectedGameProfile);
            ps.Owner = Application.Current.MainWindow;
            Application.Current.MainWindow.Opacity = 0.4;
            ps.ShowDialog();
            Application.Current.MainWindow.Opacity = 1;
        }

        public async Task DeleteProfileAsync()
        {
            try
            {
                IsEnable = false;
                bool success = await gameProfileService.DeleteProfileAsync(SelectedGameProfile.Id);
                if (success)
                {
                    string profileDir = Path.Combine(Constants.ProfilesPath, SelectedGameProfile.Id);
                    if (Directory.Exists(profileDir)) Directory.Delete(profileDir, true);
                    OnDeleteRequested?.Invoke();
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
            gameProfileService.OpenProfileFolder(SelectedGameProfile.Id);
        }

        public async Task StartGame(string serverAddress = null)
        {
            var profileToLaunch = SelectedGameProfile;
            if (profileToLaunch == null) return;
            try
            {
                profileToLaunch.IsPlaying = true;
                if (SelectedGameProfile?.Id == profileToLaunch.Id)
                {
                    IsEnable = profileToLaunch.Installed && !profileToLaunch.IsPlaying;
                    PlayButtonText = "Running";
                }
                OnLaunch.Invoke(SelectedGameProfile.Id);
                var progress = new Progress<double>(percent => ProgressNumber = percent);
                var status = new Progress<string>(status => ProgressText = status);
                var progressVisible = new Progress<bool>(progress => ProgressVisible = progress);
                await launcherService.LaunchProfileAsync(SelectedGameProfile, progress, status, progressVisible, serverAddress);
                OnLaunch.Invoke(SelectedGameProfile.Id);
            }
            finally
            {
                profileToLaunch.IsPlaying = false;
                if (SelectedGameProfile?.Id == profileToLaunch.Id)
                {
                    IsEnable = profileToLaunch.Installed && !profileToLaunch.IsPlaying;
                    PlayButtonText = "Play";
                }
            }
        }

        public void ShowContentBorwser()
        {
            ShowContentBorwserRequested.Invoke(ContentToShow.AddContent);
        }

        public async Task ExportModpackAsync()
        {
            await profileModService.ExportModpackAsync(SelectedGameProfile.Id, SelectedGameProfile.ProfileName);
        }
        public async Task ImportModpackAsync()
        {
            await profileModService.ImportModpackAsync(SelectedGameProfile.Id);
            await LoadProfileModsAsync();
        }
    }
}
