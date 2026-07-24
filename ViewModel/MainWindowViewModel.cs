using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Onova;
using Onova.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;
using TCM_Launcher.View;
using TCM_Launcher.View.PopUp;
using TCM_Launcher.View.Windows;
using TCM_Launcher.ViewModel.Popup;
using TCM_Launcher.ViewModel.UserControls;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IVersionService versionService;
        private readonly IGameProfileService gameProfileService;
        private readonly IProfileSettingsService profileSettingsService;
        private readonly IMicrosoftService microsoftService;
        private readonly IServerService serverService;
        private readonly ILauncherService launcherService;
        private readonly IProfileModService profileModService;
        private readonly IBackendService backendService;

        private readonly ProfileDetailsViewModel profileDetailsViewModel;
        private readonly AddContentViewModel addContentViewModel;
        private readonly ModDetailsViewModel modDetailsViewModel;
        public MainWindowViewModel(
            IVersionService versionService, IGameProfileService gameProfileService, IProfileSettingsService profileSettingsService, IBackendService backendService,
            IMicrosoftService microsoftService, IServerService serverService, ILauncherService launcherService, IProfileModService profileModService,
            ProfileDetailsViewModel profileDetailsViewModel, AddContentViewModel addContentViewModel, ModDetailsViewModel modDetailsViewModel)
        {
            this.versionService = versionService;
            this.gameProfileService = gameProfileService;
            this.profileSettingsService = profileSettingsService;
            this.microsoftService = microsoftService;
            this.serverService = serverService;
            this.launcherService = launcherService;
            this.profileModService = profileModService;
            this.backendService = backendService;

            this.profileDetailsViewModel = profileDetailsViewModel;
            this.profileDetailsViewModel.OnDeleteRequested = DeleteProfile;
            this.profileDetailsViewModel.ShowContentBorwserRequested = ShowAsync;
            this.profileDetailsViewModel.OnLaunch = UpdateServer;

            this.addContentViewModel = addContentViewModel;
            this.addContentViewModel.OnCloseRequested = ShowAsync;
            this.addContentViewModel.OnModDetailsRequested = ShowModDetailsAsync;

            this.modDetailsViewModel = modDetailsViewModel;
            this.modDetailsViewModel.OnBackToBrowseRequested = ShowAsync;

            InitializeAsync();
        }

        private ViewModelBase currentView;

        public ViewModelBase CurrentView
        {
            get { return currentView; }
            set { currentView = value; OnPropertyChange(); }
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

        private ObservableCollection<ServerCardViewModel> savedServers;
        public ObservableCollection<ServerCardViewModel> SavedServers
        {
            get { return savedServers; }
            set 
            {
                savedServers = value;
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

                if(selectedGameProfile != null)
                {
                    _ = ShowAsync(ContentToShow.ProfileDetails);
                }
            }
        }
        public bool HasSelectedProfile => SelectedGameProfile != null;

        private bool addProfileEnabled;
        public bool AddProfileEnabled
        {
            get { return addProfileEnabled; }
            set 
            {
                addProfileEnabled = value;
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

            versionService.StartVersionCheck();
            InternetAvailable = await NetworkUtil.IsInternetAvailableAsync();
            if(File.Exists(Constants.AccountsJSONPath) && InternetAvailable) await MicrosoftLoginAsync();
            await WaitForVersionsAsync();
        }

        public async Task OpenNewProfileWindow()
        {
            var npw = App.ServiceProvider.GetRequiredService<NewProfileView>();
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
            var profilesList = await gameProfileService.GetAllGameProfiles();
            GameProfiles = new ObservableCollection<GameProfile>(profilesList);

            var serversList = await serverService.GetSavedServersAsync();

            var serverViewModels = serversList.Select(server =>
            {
                var cardVM = App.ServiceProvider.GetRequiredService<ServerCardViewModel>();
                cardVM.Server = server;

                cardVM.OnDeleteRequested = DeleteServer;
                cardVM.OnUpdateRequested = UpdateServer;
                cardVM.OnQuickLaunchRequested = QuickStartAsync;

                return cardVM;
            });
            SavedServers = new ObservableCollection<ServerCardViewModel>(serverViewModels);
        }

        private async Task WaitForVersionsAsync()
        {
            AddProfileEnabled = false;

            if (versionService.SyncTask != null)
            {
                await versionService.SyncTask;
            }

            AddProfileEnabled = true;
        }

        private async Task<bool> CreateProfile(GameProfile data)
        {
            try
            {
                AddProfileEnabled = false;
                var p = await gameProfileService.AddProfileAsync(data.ProfileName, data.MCVersion, data.ForgeVersion);
                await profileSettingsService.SetProfileSettingsAsync(new ProfileSettings
                {
                    GameProfileId = p.Id,
                    Ram = Constants.DefaultRam,
                });
                await LoadProfilesAsync();
                SelectedGameProfile = GameProfiles.FirstOrDefault(prof => prof.Id == p.Id);
                var progressHandler = new Progress<double>(percent => profileDetailsViewModel.ProgressNumber = percent);
                var progressStatus = new Progress<string>(status => profileDetailsViewModel.ProgressText = status);
                var progressVisible = new Progress<bool>(visible => profileDetailsViewModel.ProgressVisible = visible);
                var fileName = await launcherService.CreateProfileAsync(p.Id, data.MCVersion, data.ForgeVersion, progressHandler, progressStatus, progressVisible);
                if (!string.IsNullOrEmpty(fileName))
                {
                    await gameProfileService.UpdateProfileAsync(p.Id, new GameProfile {
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
            finally
            {
                AddProfileEnabled = true;
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
        public async Task QuickStartAsync(string profileId, Server server = null)
        {
            if (string.IsNullOrEmpty(profileId)) return;

            var p = GameProfiles.FirstOrDefault(p => p.Id == profileId);
            var s = SavedServers.FirstOrDefault(s => s.Server.Id == server.Id);

            if (p == null) return;
            if(s == null) return; 

            int idx = GameProfiles.IndexOf(p);

            if (idx != -1)
            {
                p.IsPlaying = !p.IsPlaying;
                GameProfiles[idx] = p;
                SelectedGameProfile = p;
                await profileDetailsViewModel.StartGame(server.Address);
                s.PlayButtonIsEnabled = true;
            }
        }
        public void DeleteProfile()
        {
            if(SelectedGameProfile != null) {
                var serversToUnbind = SavedServers.Where(p => p.Server.BindedProfileId == SelectedGameProfile.Id).ToList();

                foreach (var s in serversToUnbind)
                {
                    int idx = SavedServers.IndexOf(s);
                    SavedServers.RemoveAt(idx);
                    s.Server.BindedProfileId = null;
                    SavedServers.Insert(idx, s);
                }
                GameProfiles.Remove(SelectedGameProfile);
            
                if (GameProfiles.Count > 0)
                {
                    SelectedGameProfile = GameProfiles[0];
                }
                else
                {
                    SelectedGameProfile = null;
                    ShowAsync(ContentToShow.None);
                }
            }
        }
        public void DeleteServer(Server s)
        {
            var vmToRemove = SavedServers.FirstOrDefault(vm => vm.Server.Id == s.Id);
            if (vmToRemove != null) SavedServers.Remove(vmToRemove);
        }
        public void UpdateServer(Server s)
        {
            var sVM = SavedServers.FirstOrDefault(ser => ser.Server.Id == s.Id);
            if (sVM != null)
            {
                sVM.Server = s;
            }
        }
        public void UpdateServer(string profileId)
        {
            var sVM = SavedServers.FirstOrDefault(ser => ser.Server.BindedProfileId == profileId);
            if (sVM != null)
            {
                sVM.PlayButtonIsEnabled = !sVM.PlayButtonIsEnabled;
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
                var manager = new UpdateManager(
                    new GithubPackageResolver("TheChillMan25", "TCM-Launcher", "TCM.Launcher.zip"),
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
            PopupView p = App.ServiceProvider.GetRequiredService<PopupView>();
            p.Initialize("Update available", "There is an update available. Click the button to download it.", PopupAction.UPDATE);
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
            var s = App.ServiceProvider.GetRequiredService<AddServerView>();
            await s.InitializeDataAsync();
            s.Owner = Application.Current.MainWindow;
            s.Owner.Opacity = 0.4;
            bool create = s.ShowDialog() ?? false;
            if(s.CreatedServer != null)
            {
                var cardVM = App.ServiceProvider.GetRequiredService<ServerCardViewModel>();
                cardVM.Server = s.CreatedServer;
                cardVM.OnDeleteRequested = DeleteServer;
                cardVM.OnUpdateRequested = UpdateServer;
                cardVM.OnQuickLaunchRequested = QuickStartAsync;
                SavedServers.Add(cardVM);
            }
            s.Owner.Opacity = 1;
        }

        public enum ContentToShow
        {
            ProfileDetails, AddContent, None
        }
        private async Task ShowAsync(ContentToShow content)
        {
            switch (content)
            {
                case ContentToShow.ProfileDetails:
                    profileDetailsViewModel.SelectedGameProfile = SelectedGameProfile;
                    await profileDetailsViewModel.LoadProfileModsAsync();
                    CurrentView = profileDetailsViewModel;
                    break;
                case ContentToShow.AddContent:
                    if (addContentViewModel.SelectedSearchResult != null) addContentViewModel.SelectedSearchResult = null;
                    if (addContentViewModel.MCVersion != SelectedGameProfile.MCVersion) addContentViewModel.MCVersion= SelectedGameProfile.MCVersion;
                    if (addContentViewModel.ProfileName != SelectedGameProfile.ProfileName) addContentViewModel.ProfileName = SelectedGameProfile.ProfileName;
                    CurrentView = addContentViewModel;
                    break;
                case ContentToShow.None:
                    CurrentView = null;
                    break;
            }
        }

        private async Task ShowModDetailsAsync(string projectId, ModSource source)
        {
            modDetailsViewModel.Details = await backendService.GetModDetailsAsync(projectId, SelectedGameProfile.MCVersion, source);
            if(modDetailsViewModel.Details == null)
            {
                MessageBox.Show("There was an error when opening the mods details.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            modDetailsViewModel.ProfileId = SelectedGameProfile.Id;
            modDetailsViewModel.MCVersion = SelectedGameProfile.MCVersion;
            await modDetailsViewModel.SetVersionsAsync();
            CurrentView = modDetailsViewModel;
        }

        public async Task MicrosoftLoginAsync()
        {
            MSession = await microsoftService.MicrosoftLoginAsync();
        }
        public async Task MicrosoftLogoutAsync()
        {
            bool success = await microsoftService.MicrosoftSignOutAsync();
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
