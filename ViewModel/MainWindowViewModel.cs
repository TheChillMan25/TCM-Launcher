using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.ViewModel.UserControls;
using TCM_Launcher.ViewModel.UserControls.Panels;
using TCM_Launcher.ViewModel.UserControls.Sidebars;
using TCML_Class_library;
using static TCM_Launcher.Core.Utils.Constants;

namespace TCM_Launcher.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IBackendService backendService;
        private readonly IVersionService versionService;
        private readonly IAppSettingsService appSettingsService;
        private readonly IOverlayService overlayService;

        private readonly ProfilesViewModel profilesViewModel;
        private ProfileDetailsViewModel profileDetailsViewModel;
        private readonly AddContentViewModel addContentViewModel;
        private readonly SearchedModDetailsViewModel modDetailsViewModel;
        private readonly AppSettingsViewModel appSettingsViewModel;

        public LeftSidebarViewModel LeftSidebarViewModel { get; }
        public RightSidebarViewModel RightSidebarViewModel { get; }
        public MainWindowViewModel(
            IBackendService backendService, IVersionService versionService, IAppSettingsService appSettingsService, IOverlayService overlayService,
            ProfileDetailsViewModel profileDetailsViewModel, AddContentViewModel addContentViewModel, SearchedModDetailsViewModel modDetailsViewModel,
            ProfilesViewModel profilesViewModel, LeftSidebarViewModel leftSidebarViewModel, RightSidebarViewModel rightSidebarViewModel, AppSettingsViewModel appSettingsViewModel)
        {
            RegisterFileAssociation();
            this.backendService = backendService;
            this.versionService = versionService;
            this.appSettingsService = appSettingsService;
            this.overlayService = overlayService;
            this.overlayService.ShowOverlayRequested += (vm) =>
            {
                ShowPanel(vm);
                IsOverlayVisible = true;
            };
            this.overlayService.CloseOverlayRequested += () =>
            {
                IsOverlayVisible = false;
                CurrentOverlay = null;
            };

            this.profilesViewModel = profilesViewModel;
            this.profilesViewModel.OnSelectProfileRequested = ShowProfileDetails;
            CurrentView = this.profilesViewModel;

            this.profileDetailsViewModel = profileDetailsViewModel;
            this.profileDetailsViewModel.OnDeleteRequested = DeleteProfile;
            this.profileDetailsViewModel.ShowContentBorwserRequested = ShowAddContent;
            this.profileDetailsViewModel.OnLaunch = OnLaunch;

            this.addContentViewModel = addContentViewModel;
            this.addContentViewModel.OnCloseRequested = ShowProfileDetails;
            this.addContentViewModel.OnModDetailsRequested = ShowModDetailsAsync;

            this.modDetailsViewModel = modDetailsViewModel;
            this.modDetailsViewModel.OnBackToBrowseRequested = ShowAddContent;
            this.modDetailsViewModel.OnModpackUpdated = UpdateProfileModpack;

            this.LeftSidebarViewModel = leftSidebarViewModel;
            this.LeftSidebarViewModel.OnHomeRequested = Show;
            this.LeftSidebarViewModel.OnAppSettingsRequested = Show;
            this.LeftSidebarViewModel.OnQuickJoinRequested = QuickStartAsync;

            this.RightSidebarViewModel = rightSidebarViewModel;

            this.appSettingsViewModel = appSettingsViewModel;

            this.appSettingsViewModel = appSettingsViewModel;
        }

        private bool initialized = false;
        private ViewModelBase currentView;
        public ViewModelBase CurrentView
        {
            get { return currentView; }
            set { currentView = value; OnPropertyChange(); }
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

        private ViewModelBase? currentOverlay;

        public ViewModelBase? CurrentOverlay
        {
            get { return currentOverlay; }
            set { currentOverlay = value; OnPropertyChange(); }
        }

        private bool isOverlayVisible;

        public bool IsOverlayVisible
        {
            get { return isOverlayVisible; }
            set { isOverlayVisible = value; OnPropertyChange(); }
        }



        //------------------------------------//

        private void ShowPanel(ViewModelBase model)
        {
            CurrentOverlay = model;
        }

        public async Task QuickStartAsync(string profileId, Server server = null)
        {
            var vm = profileDetailsViewModel.Profile?.Id == profileId
                ? profileDetailsViewModel
                : profilesViewModel.Profiles.FirstOrDefault(p => p.Profile.Id == profileId);

            if (vm != null)
            {
                vm.OnLaunch = OnLaunch;
                await vm.StartGameAsync(server?.Address);
            }
        }

        public void OnLaunch(string profileId, bool launch)
        {
            LeftSidebarViewModel.ChangePlayOnServer(profileId, !launch);
        }
        public void DeleteProfile(string id)
        {
            profilesViewModel.DeleteProfile(id);
            LeftSidebarViewModel.DeleteProfileFromServers(id);
            CurrentView = profilesViewModel;
        }

        private void UpdateProfileModpack(List<ProfileModInfo> mods)
        {
            profileDetailsViewModel.UpdateProfileMods(mods);
        }

        private void Show(ContentToShow content)
        {
            if (content == ContentToShow.Settings) CurrentView = appSettingsViewModel;
            else CurrentView = profilesViewModel;
            profileDetailsViewModel.Unsub();
        }
        private async Task ShowProfileDetails(GameProfile? p = null)
        {
            profilesViewModel.CardsEnabled = false;
            try
            {
                if (p != null) await profileDetailsViewModel.LoadProfileAsync(p);
                CurrentView = profileDetailsViewModel;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when loading profile details.", ex);
            }
            finally
            {
                profilesViewModel.CardsEnabled = true;
            }
        }
        private async Task ShowAddContent(string? profileId = null, string? name = null, string? mcVersion = null)
        {
            if (addContentViewModel.SelectedSearchResult != null) addContentViewModel.SelectedSearchResult = null;
            if (addContentViewModel.MCVersion != mcVersion && mcVersion != null) addContentViewModel.MCVersion = mcVersion;
            if (addContentViewModel.ProfileName != name && name != null) addContentViewModel.ProfileName = name;
            if (addContentViewModel.ProfileId != profileId && profileId!= null) addContentViewModel.ProfileId = profileId;
            CurrentView = addContentViewModel;
        }
        private async Task ShowModDetailsAsync(string profileId, ModSearchResult mod, string mcVersion)
        {
            modDetailsViewModel.Details = await backendService.GetModDetailsAsync(mod.Id, mod.ModrinthId, mod.CurseforgeId, mcVersion, mod.Source);
            if(modDetailsViewModel.Details == null)
            {
                MessageBox.Show("There was an error when opening the mods details.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            modDetailsViewModel.ProfileId = profileId;
            modDetailsViewModel.MCVersion = mcVersion;
            await modDetailsViewModel.SetVersionsAsync();
            CurrentView = modDetailsViewModel;
        }

        public async Task OnLoadedAsync()
        {
            await InitializeLauncherAsync();
        }

        private async Task InitializeLauncherAsync()
        {
            if(initialized) return;
            try
            {
                var vm = App.ServiceProvider.GetRequiredService<LoadingScreenViewModel>();

                overlayService.ShowLauncherLoadingPanel(vm);

                IProgress<(double progress, string status)> progress = new Progress<(double progress, string status)>(update =>
                {
                    vm.Progress = update.progress;
                    vm.Status = update.status;
                });

                progress.Report((10, "Starting initialization"));
                versionService.StartVersionCheck();
                appSettingsService.StartLoadingSettings();
                progress.Report((30, "Checking profiles and versions"));
                await profilesViewModel.Initialize();
                progress.Report((60, "Communicating with Microsoft servers"));
                await RightSidebarViewModel.MicrosoftLoginAsync(true);
                progress.Report((90, "Connecting to backend services"));
                await RightSidebarViewModel.InitializeListener();
                progress.Report((100, "Initialization successful"));
                await Task.Delay(250);
                vm.OnPanelCloseRequested?.Invoke();
                initialized = true;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception while initializing the launcher.", ex);
            }
        }

        public  void CloseButtonClick()
        {
            var behaviour = appSettingsService.AppSettings.CloseButtonBehaviour;
            appSettingsService.LauncherWindowBehaviour(behaviour, true);
        }

        private static void RegisterFileAssociation()
        {
            try
            {
                string extension = ".tcmp";
                string progId = "TCMLauncher.Modpack";
                string exePath = Environment.ProcessPath ?? "";

                using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\{extension}");
                key.SetValue("", progId);

                using var progKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}\shell\open\command");
                progKey.SetValue("", $"\"{exePath}\" \"%1\"");
            }
            catch { }
        }
    }
}
