using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using System.Reflection;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM;
using TCM_Launcher.ViewModel.UserControls;
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
        private readonly IAppMetaDataService appMetaDataService;

        private readonly ProfilesViewModel profilesViewModel;
        private ProfileDetailsViewModel profileDetailsViewModel;
        private readonly AddContentViewModel addContentViewModel;
        private readonly ModDetailsViewModel modDetailsViewModel;
        private readonly AppSettingsViewModel appSettingsViewModel;

        public LeftSidebarViewModel LeftSidebarViewModel { get; }
        public RightSidebarViewModel RightSidebarViewModel { get; }
        public MainWindowViewModel(
            IBackendService backendService, IVersionService versionService, IAppSettingsService appSettingsService, IAppMetaDataService appMetaDataService,
            ProfileDetailsViewModel profileDetailsViewModel, AddContentViewModel addContentViewModel, ModDetailsViewModel modDetailsViewModel,
            ProfilesViewModel profilesViewModel, LeftSidebarViewModel leftSidebarViewModel, RightSidebarViewModel rightSidebarViewModel, AppSettingsViewModel appSettingsViewModel)
        {
            this.backendService = backendService;
            this.versionService = versionService;
            this.appSettingsService = appSettingsService;
            this.appMetaDataService = appMetaDataService;

            this.profilesViewModel = profilesViewModel;
            this.profilesViewModel.OnSelectProfileRequested = ShowProfileDetails;
            CurrentView = this.profilesViewModel;

            this.profileDetailsViewModel = profileDetailsViewModel;
            this.profileDetailsViewModel.OnDeleteRequested = DeleteProfile;
            this.profileDetailsViewModel.ShowContentBorwserRequested = ShowAddContent;
            this.profileDetailsViewModel.OnLaunch = OnLaunch;
            this.profileDetailsViewModel.OnPinRequested = PinProfile;
            this.profileDetailsViewModel.OnProfileNameUpdated = UpdateProfileName;

            this.addContentViewModel = addContentViewModel;
            this.addContentViewModel.OnCloseRequested = ShowProfileDetails;
            this.addContentViewModel.OnModDetailsRequested = ShowModDetailsAsync;

            this.modDetailsViewModel = modDetailsViewModel;
            this.modDetailsViewModel.OnBackToBrowseRequested = ShowAddContent;
            this.modDetailsViewModel.OnModpackUpdated = UpdateProfileModpack;

            this.LeftSidebarViewModel = leftSidebarViewModel;
            this.LeftSidebarViewModel.OnHomeRequested = Show;
            this.LeftSidebarViewModel.OnAppSettingsRequested = Show;
            this.LeftSidebarViewModel.OnQuickPlayRequested = QuickStartAsync;
            this.LeftSidebarViewModel.OnUnPinRequested = UnPinProfile;

            this.RightSidebarViewModel = rightSidebarViewModel;
            this.RightSidebarViewModel.OnQuickJoinRequested = QuickStartAsync;
            this.appSettingsViewModel = appSettingsViewModel;

            this.appSettingsViewModel = appSettingsViewModel;
        }

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

        //------------------------------------//

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
            LeftSidebarViewModel.ChangePlayOnPinnedProfile(profileId, !launch);
            RightSidebarViewModel.ChangePlayOnServer(profileId, !launch);
        }
        public void DeleteProfile(string id)
        {
            profilesViewModel.DeleteProfile(id);
            LeftSidebarViewModel.DeleteProfile(id);
            RightSidebarViewModel.DeleteProfile(id);
            CurrentView = profilesViewModel;
        }

        private void UpdateProfileModpack(List<ProfileModInfo> mods)
        {
            profileDetailsViewModel.UpdateProfileMods(mods);
        }

        private void UpdateProfileName(string profileId, string name)
        {
            LeftSidebarViewModel.UpdateProfileName(profileId, name);
        }
        private async Task UnPinProfile(GameProfile profile)
        {
            var vm = profilesViewModel.Profiles.FirstOrDefault(p => p.Profile.Id == profile.Id);
            if (vm != null)
            {
                if (profileDetailsViewModel.Profile != null && 
                    profileDetailsViewModel.Profile.Id == vm.Profile.Id) await profileDetailsViewModel.PinProfile();
                else await vm.PinProfile();
            }
        }

        private void PinProfile(GameProfile p)
        {
            LeftSidebarViewModel.PinProfile(p);
        }

        private void Show(ContentToShow content)
        {
            if (content == ContentToShow.Settings) CurrentView = appSettingsViewModel;
            else CurrentView = profilesViewModel;
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
            versionService.StartVersionCheck();
            appSettingsService.StartLoadingSettings();
        }

        public  void CloseButtonClick()
        {
            var behaviour = appSettingsService.AppSettings.CloseButtonBehaviour;
            appSettingsService.LauncherWindowBehaviour(behaviour, true);
        }
    }
}
