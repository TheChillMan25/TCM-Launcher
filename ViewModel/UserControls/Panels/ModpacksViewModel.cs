using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TCM_Launcher.Interfaces;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel.UserControls.Panels
{
    public class ModpacksViewModel : ViewModelBase
    {
        private readonly IFirebaseService firebaseService;
        private readonly IMicrosoftService microsoftService;
        private readonly IBackendService backendService;
        public ModpacksViewModel(IFirebaseService firebaseService, IMicrosoftService microsoftService, IBackendService backendService)
        {
            this.firebaseService = firebaseService;
            this.microsoftService = microsoftService;
            this.backendService = backendService;
        }
        private FirestoreModpack? modpackToShare;
        public Action? OnPanelCloseRequested { get; set; }

        private ObservableCollection<ModpackViewModel> modpacks = new();
        public ObservableCollection<ModpackViewModel> Modpacks
        {
            get { return modpacks; }
            set { modpacks = value; }
        }

        private bool shareWithFriends;

        public bool SharePanelVisible
        {
            get { return shareWithFriends; }
            set 
            { 
                shareWithFriends = value; 
                OnPropertyChange(); 
                if(value == false)
                {
                    foreach(var f in SelectableFriends)
                    {
                        f.IsSelected = false;
                    }
                }
            }
        }


        private List<SelectableFriendViewModel> selectableFriends;

        public List<SelectableFriendViewModel> SelectableFriends
        {
            get { return selectableFriends; }
            set { selectableFriends = value; OnPropertyChange(); }
        }


        public void Close()
        {
            OnPanelCloseRequested?.Invoke();
        }

        public async Task Initialize()
        {
            if (microsoftService.MSession == null) return;
            var list = firebaseService.CachedModpacks.Select(m =>
            {
                var vm = App.ServiceProvider.GetRequiredService<ModpackViewModel>();
                vm.Initialize(m);
                vm.OnShareRequested = ShowSharePanel;
                vm.OnDeleteRequested = DeleteModpack;
                return vm;
            });
            Modpacks = new(list);
        }

        private void DeleteModpack(string id)
        {
            var existring = Modpacks.FirstOrDefault(m => m.Id == id);
            if (existring != null) Modpacks.Remove(existring);
        }

        private async Task ShowSharePanel(FirestoreModpack modpackToShare, List<string> sharedWith)
        {
            this.modpackToShare = modpackToShare;
            SelectableFriends = firebaseService.CachedFriends.Where(u => !sharedWith.Contains(u.UUID)).Select(f =>  new SelectableFriendViewModel(f)).ToList();
            SharePanelVisible = true;
        }

        public void CloseSharePanel()
        {
            SharePanelVisible = false;
        }

        public async Task ShareModpackAsync()
        {
            if (modpackToShare == null) return;
            var selected = SelectableFriends.Where(f => f.IsSelected).Select(f => f.User.UUID).ToList();
            var success = await backendService.ShareModpackAsync(modpackToShare, selected);
            if (success) SharePanelVisible = false;
            this.modpackToShare = null;
        }
    }
}
