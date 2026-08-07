using Microsoft.Extensions.DependencyInjection;
using Onova;
using Onova.Services;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.View.PopUp;
using TCM_Launcher.ViewModel.Popup;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using static TCM_Launcher.Core.Utils.Constants;

namespace TCM_Launcher.ViewModel.UserControls.Sidebars
{
    public class LeftSidebarViewModel : ViewModelBase
    {
        private readonly IGameProfileService gameProfileService;
        private readonly IAppMetaDataService appMetaDataService;
        public LeftSidebarViewModel(IGameProfileService gameProfileService, IAppMetaDataService appMetaDataService)
        {
            this.gameProfileService = gameProfileService;
            this.appMetaDataService = appMetaDataService;
        }

        public Action<ContentToShow>? OnHomeRequested { get; set; }
        public Action<ContentToShow>? OnAppSettingsRequested { get; set; }
        public Func<string, Server, Task>? OnQuickPlayRequested { get; set; }

        private UpdateData availableUpdate = new UpdateData();
        public UpdateData AvailableUpdate
        {
            get { return availableUpdate; }
            set
            {
                availableUpdate = value;
                OnPropertyChange();
            }
        }

        private ObservableCollection<PinnedProfileViewModel> profiles = new ObservableCollection<PinnedProfileViewModel>();
        public ObservableCollection<PinnedProfileViewModel> Profiles
        {
            get { return profiles; }
            set { profiles = value; OnPropertyChange(); }
        }

        public Func<GameProfile, Task>? OnUnPinRequested { get; set; }

        public string AppVersion 
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return version != null ? $"v{version?.Major}.{version?.Minor}.{version?.Build}" :
                    "v0.0.1";
            } 
        }

        public async Task InitializeAsync()
        {
            var loadProfilesTask = LoadPinnedProfilesAsync();
            var checkUpdatesTask = CheckForUpdatesAsync();

            await Task.WhenAll(loadProfilesTask, checkUpdatesTask);
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
                        await Update();
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
                AvailableUpdate.IsUpdating = true;
                //await appMetaDataService.AddMetaData("app_version", AvailableUpdate.Version.ToString());
                await AvailableUpdate.Manager.PrepareUpdateAsync(AvailableUpdate.Version);
                AvailableUpdate.Manager.LaunchUpdater(AvailableUpdate.Version);
                Application.Current.Shutdown();
            }
        }
        public async Task LoadPinnedProfilesAsync()
        {
            var p = await gameProfileService.GetPinnedProfilesAsync();
            var vmList = p.Select(p =>
            {
                var vm = App.ServiceProvider.GetRequiredService<PinnedProfileViewModel>();
                vm.Profile = p;
                vm.OnQuickPlayRequested = QuickPlay;
                vm.OnUnPinRequested = RemovePinnedProfile;
                return vm;
            }).ToList();
            Profiles = new ObservableCollection<PinnedProfileViewModel>(vmList);
        }

        public void ShowHome()
        {
            OnHomeRequested?.Invoke(ContentToShow.Home);
        }

        public void PinProfile(GameProfile profile)
        {
            try
            {
                var epCM = Profiles.FirstOrDefault(p => p.Profile.Id == profile.Id);
                if (epCM == null || epCM.Profile.Pinned == false)
                {
                    var vm = App.ServiceProvider.GetRequiredService<PinnedProfileViewModel>();
                    vm.Profile = profile;
                    vm.OnQuickPlayRequested = QuickPlay;
                    vm.OnUnPinRequested = RemovePinnedProfile;
                    Profiles.Add(vm);
                }
                else if(epCM.Profile.Pinned == true)
                {
                    Profiles.Remove(epCM);
                }
            }
            catch(Exception ex)
            {
                Logger.Error("There was an exception when pinning profile.", ex);
            }
        }

        private void QuickPlay(string profileId)
        {
            OnQuickPlayRequested?.Invoke(profileId, null);
        }

        private void RemovePinnedProfile(GameProfile profile)
        {
            OnUnPinRequested?.Invoke(profile);
        }

        public void ChangePlayOnPinnedProfile(string profileId, bool value = true)
        {
            var p = Profiles.FirstOrDefault(p => p.Profile.Id == profileId);
            if(p != null)
            {
                p.PlayEnabled = value;
            }
        }

        public void ShowAppSettings()
        {
            OnAppSettingsRequested?.Invoke(ContentToShow.Settings);
        }

        public void DeleteProfile(string profileId)
        {
            var existing = Profiles.FirstOrDefault(p =>p.Profile.Id == profileId);
            if (existing != null)
            {
                Profiles.Remove(existing);
            }
        }

        public void UpdateProfileName(string profileId, string name)
        {
            var existing = Profiles.FirstOrDefault(p => p.Profile.Id == profileId);
            if(existing != null)
            {
                existing.UpdateProfileName(name);
            }
        }
    }
}
