using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using Onova;
using Onova.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;
using TCM_Launcher.View;
using TCM_Launcher.View.PopUp;
using TCM_Launcher.View.Windows;

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

        private ObservableCollection<Server> savedServers;/* = new ObservableCollection<Server>([new Server { 
            Name = "Server name", 
            MCVersion = "1.20.1", 
            BindedProfileId = "", 
            Id = 1, 
            BindedProfile = null, 
            IP="192.168.1.200"}]);*/

        public ObservableCollection<Server> SavedServers
        {
            get { return savedServers; }
            set 
            {
                savedServers = value;
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

        private JELoginHandler loginHandler;
        public JELoginHandler LoginHandler
        {
            get { return loginHandler; }
            set { loginHandler = value; }
        }

        private bool internetAvailable;
        public bool InternetAvailable
        {
            get { return internetAvailable; }
            set 
            { 
                internetAvailable = value;
                OnPropertyChange();
            }
        }

        private MSession mSession;
        public MSession MSession
        {
            get { return mSession; }
            set 
            { 
                mSession = value;
                OnPropertyChange();
                OnPropertyChange(nameof(LoggedIntoMSAccount));
            }
        }
        public bool LoggedIntoMSAccount => MSession != null;

        //------------------------------------//

        private async void InitializeAsync()
        {
            await LoadProfilesAsync();

            OpenLastPlayedProfile();

            VersionsService.Instance.StartVersionCheck();
            InternetAvailable = await NetworkUtil.IsInternetAvailableAsync();
            if(File.Exists(Constants.AccountsJSONPath) && InternetAvailable) await MicrosoftLoginAsync();
            await WaitForVersionsAsync();
        }

        public async Task OpenNewProfileWindow()
        {
            NewProfileView npw = new NewProfileView();
            npw.Owner = Application.Current.MainWindow;
            Application.Current.MainWindow.Opacity = 0.4;
            bool created = npw.ShowDialog() ?? false;
            Application.Current.MainWindow.Opacity = 1;
            if (created) 
            {
                bool isInternet = await NetworkUtil.IsInternetAvailableAsync();
                if (!isInternet)
                {
                    Logger.Error("No internet. Couldn't check versions.");
                    MessageBox.Show("Cannot download files. Connect to internet", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                bool success = await CreateProfile(npw.NewProfileData);

                if (success) MessageBox.Show("Installation complete");
            }
        }

        public async Task LoadProfilesAsync()
        {
            var profilesList = await GameProfileService.Instance.GetAllGameProfiles();
            GameProfiles = new ObservableCollection<GameProfile>(profilesList);
            var serversList = await ServerService.Instance.GetSavedServersAsync();
            SavedServers = new ObservableCollection<Server>(serversList);
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
                var p = await GameProfileService.Instance.AddProfileAsync(data.ProfileName, data.MCVersion, data.ForgeVersion);
                await ProfileSettingsService.Instance.SetProfileSettingsAsync(new ProfileSettings
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
                    await GameProfileService.Instance.UpdateProfileAsync(p.Id, new GameProfile {
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
                MessageBox.Show($"An error occured during profile creation.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
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
        public void DeleteServer(Server s)
        {
            SavedServers.Remove(s);
        }
        public void UpdateServer(Server s)
        {
            var oldServer = SavedServers.FirstOrDefault(ser => ser.Id == s.Id);
            if (oldServer != null)
            {
                int idx = SavedServers.IndexOf(oldServer);
                SavedServers[idx] = s;
            }
        }
        public void UpdateProfile(string id)
        {
            var p = GameProfiles.FirstOrDefault(p => p.Id == id);
            int idx = GameProfiles.IndexOf(p);
            p.IsPlaying = !p.IsPlaying;
            GameProfiles[idx] = p;
            SelectedGameProfile = p;
        }
        public async Task CheckForUpdatesAsync()
        {
            //#if DEBUG
            //    Console.WriteLine("Developer mode: Update check cancelled.");
            //    return;
            //#endif

            try
            {
                var manager = new UpdateManager(
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
                var tmpUpdate = AvailableUpdate;
                tmpUpdate.IsUpdating = true;
                AvailableUpdate = tmpUpdate;
                await AvailableUpdate.Manager.PrepareUpdateAsync(AvailableUpdate.Version);

                AvailableUpdate.Manager.LaunchUpdater(AvailableUpdate.Version);
                Application.Current.Shutdown();
            }
        }
        public void Bugreport()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Constants.BugReportFormURL,
                    UseShellExecute = true
                });
                if (Directory.Exists(Path.Combine(Constants.LauncherFolder, "logs")))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        UseShellExecute = true,
                        Arguments = Path.Combine(Constants.LauncherFolder, "logs")
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception during bugreport", ex);
                MessageBox.Show("An error occured during bugreport.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task OpenAddServerWindowAsync()
        {
            AddServerView s = new AddServerView();
            await s.InitializeDataAsync();
            s.Owner = Application.Current.MainWindow;
            s.Owner.Opacity = 0.4;
            bool create = s.ShowDialog() ?? false;
            if(s.CreatedServer != null)
            {
                SavedServers.Add(s.CreatedServer);
            }
            s.Owner.Opacity = 1;
        }

        public async Task MicrosoftLoginAsync()
        {
            MSession = await MicrosoftService.Instance.MicrosoftLoginAsync();
        }
        public async Task MicrosoftLogoutAsync()
        {
            bool success = await MicrosoftService.Instance.MicrosoftSignOutAsync();
            if (success)
            {
                MSession = null;
            }
        }
        public async Task CheckNetworkAsync()
        {
            InternetAvailable = await NetworkUtil.IsInternetAvailableAsync();
        }
    }
}
