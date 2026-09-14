using System.Collections.ObjectModel;
using System.Windows;
using System.Xml.Linq;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.DB.Versions;
using TCM_Launcher.MVVM.ViewModel;

namespace TCM_Launcher.ViewModel.UserControls.Panels
{
    public class NewProfileViewModel : ViewModelBase
    {
        public Action<GameProfile?>? OnPanelCloseRequested { get; set; }
        private readonly IVersionService versionService;
        public NewProfileViewModel(IVersionService versionService)
        {
            this.versionService = versionService;
        }

        private VanillaVersion vanilla;

        public VanillaVersion Vanilla
        {
            get { return vanilla; }
            set { vanilla = value; OnPropertyChange(); }
        }
        private ForgeVersion forge;

        public ForgeVersion Forge
        {
            get { return forge; }
            set { forge = value; OnPropertyChange(); }
        }


        private ObservableCollection<VanillaVersion> versions;

        public ObservableCollection<VanillaVersion> Versions
        {
            get { return versions; }
            set 
            { 
                versions = value;
                OnPropertyChange();
            }
        }

        private ObservableCollection<ForgeVersion> forgeVersions;

        public ObservableCollection<ForgeVersion> ForgeVersions
        {
            get { return forgeVersions; }
            set 
            {
                forgeVersions = value;
                OnPropertyChange();
            }
        }

        private bool hasForgeVersions;

        public bool HasForgeVersions
        {
            get { return hasForgeVersions; }
            set {
                hasForgeVersions = value;
                OnPropertyChange();
            }
        }

        private bool canCreate = true;

        public bool CanCreate
        {
            get { return canCreate; }
            set {
                canCreate = value;
                OnPropertyChange();
            }
        }


        private string profileName;

        public string ProfileName
        {
            get { return profileName; }
            set
            {
                profileName = value;
                OnPropertyChange();
            }

        }

        public async Task InitializeVersions()
        {
            if (versionService.SyncTask != null)
            {
                await versionService.SyncTask;
            }
            var loadedVersions = await versionService.GetVanillaVersions();

            Versions = new ObservableCollection<VanillaVersion>(loadedVersions);
            ForgeVersions = new();
        }

        public async Task UpdateForgeVersions(string mcVersion, string pName = "")
        {
            try
            {
                var versions = await versionService.GetForgeVersions(mcVersion);
                ForgeVersions = new ObservableCollection<ForgeVersion>(versions);
                HasForgeVersions = ForgeVersions.Count > 0;
                if (string.IsNullOrEmpty(pName) || pName.Contains("Forge")) ProfileName = string.Concat("Forge ", mcVersion);
            }
            catch (Exception ex)
            {
                HasForgeVersions = false;
                Logger.Error($"There was an exception during updating forge versions for minecraft {mcVersion}", ex);
                MessageBox.Show($"An error occured during fetching forge versions.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Close()
        {
            OnPanelCloseRequested?.Invoke(null);
        }

        public void CreateProfile()
        {
            var p = new GameProfile
            {
                ProfileName = string.IsNullOrEmpty(ProfileName) ? $"Forge {Vanilla.VersionName}" : ProfileName,
                MCVersion = vanilla.VersionName,
                ForgeVersion = forge.VersionName,
            };
            OnPanelCloseRequested?.Invoke(p);
        }
    }
}
