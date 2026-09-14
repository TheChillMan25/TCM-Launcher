using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.View;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class ProfilesViewModel : ViewModelBase
    {
        private readonly IGameProfileService gameProfileService;
        private readonly IProfileSettingsService profileSettingsService;
        private readonly ILauncherService launcherService;
        private readonly IVersionService versionService;
        private readonly IProfileModService profileModService;
        private readonly IOverlayService overlayService;
        private readonly IFirebaseService firebaseService;
        public ICommand SelectProfileCommand { get; }

        public ProfilesViewModel(IGameProfileService gameProfileService, IProfileSettingsService profileSettingsService, 
            ILauncherService launcherService, IVersionService versionService, IProfileModService profileModService, IOverlayService overlayService, IFirebaseService firebaseService)
        {
            this.gameProfileService = gameProfileService;
            this.profileSettingsService = profileSettingsService;
            this.launcherService = launcherService;
            this.versionService = versionService;
            this.profileModService = profileModService;
            this.overlayService = overlayService;

            this.SelectProfileCommand = new RelayCommand<GameProfile>(SelectProfile);
            this.firebaseService = firebaseService;
        }

        private bool isInitialized = false;

        public Func<GameProfile, Task> OnSelectProfileRequested { get; set; }

        private ObservableCollection<ProfileDetailsViewModel> profiles;
		public ObservableCollection<ProfileDetailsViewModel> Profiles
		{
			get { return profiles; }
			set { profiles = value; OnPropertyChange(); }
		}

        private ObservableCollection<ProfileInstallIndicatorViewModel> installs = new();

        public ObservableCollection<ProfileInstallIndicatorViewModel> Installs
        {
            get { return installs; }
            set { installs = value; OnPropertyChange(); }
        }


        private bool addProfileEnabled = false;
        public bool AddProfileEnabled
        {
            get { return addProfileEnabled; }
            set { addProfileEnabled = value; OnPropertyChange(); }
        }

        private bool cardsEnabled = true;
        public bool CardsEnabled
        {
            get { return cardsEnabled; }
            set { cardsEnabled = value; OnPropertyChange(); }
        }


        public async Task Initialize()
        {
            if(isInitialized) return;
            try
            {
                var profiles = await gameProfileService.GetAllGameProfiles();
                var vmList = profiles.Select(p =>
                {
                    var vm = App.ServiceProvider.GetRequiredService<ProfileDetailsViewModel>();
                    vm.Profile = p;
                    return vm;
                }).ToList();
                Profiles = new ObservableCollection<ProfileDetailsViewModel>(vmList);
                await WaitForVersionsAsync();
                isInitialized = true;
            }
            catch(Exception ex)
            {
                Logger.Error("There was an exception during profiles initialization.", ex);
            }
        }

        private void SelectProfile(GameProfile p)
        {
            OnSelectProfileRequested?.Invoke(p);
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

        public async Task OpenNewProfileWindow()
        {
            GameProfile result = await overlayService.ShowNewProfilePanelAsync();
            if (result != null)
            {
                bool isInternet = await NetworkUtil.IsInternetAvailableAsync();
                if (!isInternet)
                {
                    Logger.Error("No internet. Couldn't check versions.");
                    Constants.MessageBoxError("Cannot download files. Connect to internet.");
                    return;
                }
                await CreateProfileAsync(result);
            }
        }

        private async Task<bool> CreateProfileAsync(GameProfile data)
        {
            try
            {
                AddProfileEnabled = false;
                var p = await gameProfileService.AddProfileAsync(data.ProfileName, data.MCVersion, data.ForgeVersion);
                var pTask = profileSettingsService.SetProfileSettingsAsync(new ProfileSettings
                {
                    GameProfileId = p.Id,
                    Ram = Constants.DefaultRam,
                });

                var installVm = App.ServiceProvider.GetRequiredService<ProfileInstallIndicatorViewModel>();

                var progress = new Progress<double>(progress => installVm.Progress = progress);
                var status = new Progress<string>(status => installVm.Status = status);
                var visible = new Progress<bool>(visible => installVm.Visible = visible);

                installVm.ProfileName = p.ProfileName;
                installVm.ProfileId = p.Id;
                installVm.OnRemoveRequested = () =>
                {
                    var existing = Installs.FirstOrDefault(vm => vm.ProfileId == installVm.ProfileId);
                    if (existing != null) Installs.Remove(existing);
                };
                Installs.Add(installVm);

                var fileNameTask = launcherService.CreateProfileAsync(p.Id, data.MCVersion, data.ForgeVersion, progress, status, visible);
                await Task.WhenAll( pTask, fileNameTask );
                if (!string.IsNullOrEmpty(fileNameTask.Result))
                {
                    p.FileName = fileNameTask.Result;
                    p.Installed = true;
                    await gameProfileService.UpdateProfileAsync(p);
                    var vm = App.ServiceProvider.GetRequiredService<ProfileDetailsViewModel>();
                    vm.Profile = p;
                    Profiles.Add(vm);
                    await profileModService.CreateProfileManifest(p.Id, p.ProfileName, p.MCVersion, p.ModpackId, p.ForgeVersion);
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

        public void DeleteProfile(string id)
        {
            var p = Profiles.FirstOrDefault(p => p.Profile.Id == id);
            if (p != null)
            {
                Profiles.Remove(p);
            }
        }
    }
}
