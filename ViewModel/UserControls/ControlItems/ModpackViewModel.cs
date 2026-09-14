using TCM_Launcher.Interfaces;
using TCM_Launcher.MVVM.ViewModel;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class ModpackViewModel : ViewModelBase
    {
        private readonly IBackendService backendService;
        private readonly IMicrosoftService microsoftService;
        public ModpackViewModel(IBackendService backendService, IMicrosoftService microsoftService)
        {
            this.backendService = backendService;
            this.microsoftService = microsoftService;
        }

        public Func<FirestoreModpack, List<string>, Task> OnShareRequested { get; set; }
        public Action<string> OnDeleteRequested { get; set; }

        private FirestoreModpack modpack;

        public string Id { get; set; }
        public string OwnerName { get; set; }
        public string OwnerUUID { get; set; }
        public string ModpackName { get; set; }
        public string ModpackVersion { get; set; }
        public string StoragePath { get; set; }
        public List<string> SharedWith { get; set; }
        public bool IsOwner { get; private set; }

        private bool buttonsEnabled = true;
        public bool ButtonsEnabled
        {
            get { return buttonsEnabled; }
            set { buttonsEnabled = value; OnPropertyChange(); }
        }
        public void Initialize(FirestoreModpack modpack)
        {
            this.modpack = modpack;
            Id = modpack.Id;
            OwnerName = modpack.OwnerName;
            OwnerUUID = modpack.OwnerUUID;
            ModpackName = modpack.Name;
            ModpackVersion = modpack.Version;
            StoragePath = modpack.StoragePath;
            SharedWith = modpack.SharedWith;
            IsOwner = OwnerUUID.Equals(microsoftService.MSession.UUID);
        }

        public async Task CreateProfileAsync()
        {
            ButtonsEnabled = false;
            await backendService.DownloadModpackAsync(modpack);
            ButtonsEnabled = true;
        }

        public async Task DeleteModpackAsync()
        {
            ButtonsEnabled = false;
            await backendService.DeleteModpackAsync(modpack);
            OnDeleteRequested?.Invoke(Id);
            ButtonsEnabled = true;
        }

        public void ShareModpack()
        {
            OnShareRequested?.Invoke(this.modpack, SharedWith);
        }
    }
}
