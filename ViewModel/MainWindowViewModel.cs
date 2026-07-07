using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using TCM_Launcher.Core;
using TCM_Launcher.Model.DB;
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
            _ = LoadProfilesAsync();
            OpenLastPlayedProfile();
            VersionsService.Instance.StartVersionCheck();
            _ = WaitForVersionsAsync();
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

        private bool isVersionsLoaded;

        public bool IsVersionsLoaded
        {
            get { return isVersionsLoaded; }
            set 
            {
                isVersionsLoaded = value;
                OnPropertyChange();
                OnPropertyChange(nameof(IsVersionsLoading));
            }
        }
        public bool IsVersionsLoading => !IsVersionsLoaded;

        private double downloadProgress;
        public double DownloadProgress
        {
            get { return downloadProgress; }
            set
            { 
                downloadProgress = value;
                OnPropertyChange();
            }
        }

        private bool isDownloading;
        public bool IsDownloading
        {
            get { return isDownloading; }
            set 
            {
                isDownloading = value; 
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

        public async Task LoadProfilesAsync()
        {
            var profilesList = await GameProfileService.Instance.GetAllGameProfiles();
            GameProfiles = new ObservableCollection<GameProfile>(profilesList);
        }

        private async Task WaitForVersionsAsync()
        {
            IsVersionsLoaded = false;

            if (VersionsService.Instance.SyncTask != null)
            {
                await VersionsService.Instance.SyncTask;
            }

            IsVersionsLoaded = true;
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
                Application.Current.MainWindow.Opacity = 1;
                await LoadProfilesAsync();
                IsDownloading = true;
                DownloadProgress = 0;
                var progressHandler = new Progress<double>(percent =>
                {
                    DownloadProgress = percent;
                });
                var fileName = await LauncherService.Instance.CreateProfileAsync(p.Id, data.MCVersion, data.ForgeVersion, progressHandler);
                IsDownloading = false;
                DownloadProgress = 0;
                if (!string.IsNullOrEmpty(fileName))
                {
                    GameProfileService.Instance.UpdateProfile(p.Id, new GameProfile {
                        ProfileName = p.ProfileName,
                        MCVersion = p.MCVersion,
                        ForgeVersion = p.ForgeVersion,
                        FileName = fileName,
                        Installed = true 
                    });
                    await LoadProfilesAsync();
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
                    if (Directory.Exists(profileDir)) Directory.Delete(profileDir, true);
                    GameProfiles.Remove(SelectedGameProfile);

                    if (GameProfiles.Count > 0)
                    {
                        SelectedGameProfile = GameProfiles[0];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured during deleting profile.");
            }

        }
    }
}
