using System.Collections.ObjectModel;
using System.Windows.Data;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.ViewModel.UserControls.Panels
{
    public class ExportModpackViewModel : ViewModelBase
    {
        private readonly IProfileModService profileModService;
        private readonly IFirebaseService firebaseService;
        private readonly IBackendService backendService;
        private readonly IGameProfileService gameProfileService;
        private readonly IDownloadedModpacksService downloadedModpacksService;
        public string ProfileId { private get; set; }
        public string ProfileName { private get; set; }
        public string ProfileVersion { private get; set; }
        public string? ProfileModpackId { private get; set; }
        public string ButtonText { get; set; }
        public ExportModpackViewModel(IProfileModService profileModService, IFirebaseService firebaseService, IBackendService backendService, IDownloadedModpacksService downloadedModpacksService, IGameProfileService gameProfileService)
        {
            this.profileModService = profileModService;
            this.firebaseService = firebaseService;
            this.backendService = backendService;
            this.downloadedModpacksService = downloadedModpacksService;
            this.gameProfileService = gameProfileService;
        }

        public Action<string?>? OnPanelCloseRequested { get; set; }

        private bool exportJARFiles = true;
        public bool ExportJARFiles
        {
            get { return exportJARFiles; }
            set 
            { 
                exportJARFiles = value; 
                OnPropertyChange();
                if (value == false) ExportOnlyImportedFiles = false;
            }
        }

        private bool exportOnlyImportedFiles;
        public bool ExportOnlyImportedFiles
        {
            get { return exportOnlyImportedFiles; }
            set { exportOnlyImportedFiles = value; OnPropertyChange(); }
        }

        private bool shareWithFriends = false;
        public bool ShareWithFriends
        {
            get { return shareWithFriends; }
            set 
            { 
                shareWithFriends = value; 
                OnPropertyChange();
                if (value == false)
                {
                    foreach (var friend in selectableFriends)
                    {
                        friend.IsSelected = false;
                    }
                }
                SharePanelVisible = value;
            }
        }

        private ObservableCollection<ProfileModViewModel> exportableMods;
		public ObservableCollection<ProfileModViewModel> ExportableMods
		{
			get { return exportableMods; }
			set { exportableMods = value; OnPropertyChange(); }
		}

        private List<SelectableFriendViewModel> selectableFriends = new();
        public List<SelectableFriendViewModel> SelectableFriends
        {
            get { return selectableFriends; }
            set { selectableFriends = value; }
        }

        public bool IsUpdate { get; private set; }

        private bool sharePanelVisible = false;
        public bool SharePanelVisible
        {
            get { return sharePanelVisible; }
            set 
            { 
                sharePanelVisible = value; 
                OnPropertyChange();
            }
        }

        public async Task Initialize(GameProfile profile, List<ProfileModViewModel> mods)
        {
            ProfileId = profile.Id;
            ProfileName = profile.ProfileName;
            ProfileVersion = profile.MCVersion;
            ExportableMods = new(mods);
            ProfileModpackId = profile.ModpackId;
            SelectableFriends = firebaseService.CachedFriends.Select(f => new SelectableFriendViewModel(f)).ToList();
            IsUpdate = await downloadedModpacksService.IsExistingModpackAsync(profile.ModpackId);
            if(IsUpdate == true)
            {
                ButtonText = "Update";
            }
            else
            {
                ButtonText = "Export";
            }
        }

        public void ToggleModExport(string modId, bool isChecked)
        {
            var existing = ExportableMods.FirstOrDefault(m => m.Mod.Id == modId);
            if(existing != null)
            {
                existing.Mod.IsEnabled = isChecked;
                OnPropertyChange(nameof(existing.Mod.IsEnabled));
            }
        }

        public void ToggleAllModsExport(bool isChecked)
        {
            foreach (var model in ExportableMods)
            {
                if(model.Mod.IsEnabled != isChecked) model.Mod.IsEnabled = isChecked;
            }
            CollectionViewSource.GetDefaultView(ExportableMods)?.Refresh();
        }

        public async Task ExportModpackAsync()
        {
            var selectedFriends = SelectableFriends.Where(f => f.IsSelected).Select(f => f.User.UUID).ToList();
            if(shareWithFriends && selectedFriends.Count == 0)
            {
                Constants.MessageBoxError("Select atleast one friend to share with.");
                return;
            }
            var filePath = await profileModService.ExportModpackAsync(ProfileId, ProfileName, ExportJARFiles, ExportableMods.Select(vm => vm.Mod).ToList(), ExportOnlyImportedFiles);
            if(string.IsNullOrEmpty(filePath))
            {
                Constants.MessageBoxError("Modpack export failed.");
                return;
            }
            string? packId = null;
            if (selectedFriends.Count > 0)
            {
                var modpack = await backendService.UploadModpackAsync(filePath, ProfileName, ProfileVersion, ProfileModpackId, selectedFriends, IsUpdate);
                if (modpack == null)
                {
                    Constants.MessageBoxError("Modpack upload failed.");
                    return;
                }
                packId = modpack.Id;
                await downloadedModpacksService.AddModpackAsync(modpack);
                await gameProfileService.UpdateProfileAsync(new GameProfile { Id = ProfileId, ModpackId = modpack.Id });
            }
            Close(packId);
        }

        public void Close(string? packId = null)
        {
            OnPanelCloseRequested?.Invoke(packId);
        }
    }
}
