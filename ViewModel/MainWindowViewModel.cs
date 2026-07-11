using Onova;
using Onova.Services;
using System.Collections.ObjectModel;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;
using TCM_Launcher.View;
using TCM_Launcher.View.PopUp;

namespace TCM_Launcher.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            InitializeAsync();
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

        public GameProfile? SelectedGameProfile
        {
            get { return selectedGameProfile; }
            set
            {
                selectedGameProfile = value;
                OnPropertyChange();
                OnPropertyChange(nameof(HasSelectedProfile));
            }
        }
        public bool HasSelectedProfile => SelectedGameProfile != null;

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

        private UpdateData availableUpdtea = new UpdateData();

        public UpdateData AvailableUpdate
        {
            get { return availableUpdtea; }
            set 
            {
                availableUpdtea = value;
                OnPropertyChange();
            }
        }


        private async void InitializeAsync()
        {
            await LoadProfilesAsync();

            OpenLastPlayedProfile();

            VersionsService.Instance.StartVersionCheck();
            await WaitForVersionsAsync();
        }

        public async Task ShowNewProfileWindow()
        {
            NewProfileView npw = new NewProfileView();
            npw.Owner = Application.Current.MainWindow;
            Application.Current.MainWindow.Opacity = 0.4;
            bool created = npw.ShowDialog() ?? false;
            Application.Current.MainWindow.Opacity = 1;
            if (created) 
            {
                bool success = await CreateProfile(npw.NewProfileData);

                if (success) MessageBox.Show("Installation complete");
            }
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
                await LoadProfilesAsync();
                SelectedGameProfile = GameProfiles.FirstOrDefault(prof => prof.Id == p.Id);
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
                Logger.Error("There was an exception during profile creation", ex);
                MessageBox.Show($"An error occured during profile creation; {ex.Message}");
                return false;
            }
        }

        private void OpenLastPlayedProfile()
        {
            if (GameProfiles == null || GameProfiles.Count == 0) return;
            var lastPlayed = GameProfiles.FirstOrDefault(p => p.LastPlayed == true);

            if (lastPlayed != null)
            {
                SelectedGameProfile = lastPlayed;
            }
            else
            {
                SelectedGameProfile = GameProfiles[0];
            }
        }

        public void DeleteProfile()
        {
            if(SelectedGameProfile != null) {
                GameProfiles.Remove(SelectedGameProfile);
            
                if (GameProfiles.Count > 0)
                {
                    SelectedGameProfile = GameProfiles[0];
                }
                else SelectedGameProfile = null;
            }
        }

        public async Task CheckForUpdatesAsync()
        {
            #if DEBUG
                Console.WriteLine("Developer mode: Update check cancelled.");
                return;
            #endif

            try
            {
                using var manager = new UpdateManager(
                    new GithubPackageResolver("TheChillMan25", "TCM-Launcher", "TCM_Launcher.zip"),
                    new ZipPackageExtractor()
                );
                AvailableUpdate.Manager = manager;

                var check = await manager.CheckForUpdatesAsync();
                if (check != null) 
                {
                    AvailableUpdate.CanUpdate = check.CanUpdate;
                    OnPropertyChange(nameof(AvailableUpdate));
                    if (AvailableUpdate.CanUpdate)
                    {
                        AvailableUpdate.Version = check.LastVersion ?? check.Versions.FirstOrDefault();
                        Update();
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exceprtion during checking for updates", ex);
            }
        }
        public async Task Update()
        {
            PopupView p = new PopupView("Update available", "There is an update available. Click the button to download it.", UI.Popup.PopupAction.UPDATE);
            p.Owner = Application.Current.MainWindow;
            var update = p.ShowDialog();
            if (update == true)
            {
                await AvailableUpdate.Manager.PrepareUpdateAsync(AvailableUpdate.Version);

                AvailableUpdate.Manager.LaunchUpdater(AvailableUpdate.Version);
                Application.Current.Shutdown();
            }
        }
    }
}
