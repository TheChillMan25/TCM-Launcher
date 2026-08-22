using System.Collections.ObjectModel;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB.Versions;
using TCM_Launcher.MVVM;

namespace TCM_Launcher.ViewModel.Windows
{
    public class NewProfileViewModel : ViewModelBase
    {
        private readonly IVersionService versionService;
        public NewProfileViewModel(IVersionService versionService)
        {
            this.versionService = versionService;
            InitializeVersions();
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


        private string placeholderProfileName;

        public string PlaceholderProfileName
        {
            get { return placeholderProfileName; }
            set
            {
                placeholderProfileName = value;
                OnPropertyChange();
            }

        }

        private async Task InitializeVersions()
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
                /*ForgeLoaderJSONData recommended = null;
                if (versions.Count > 0 ) recommended = versions.FirstOrDefaultAsync(v => v.IsRecommended);
                if (recommended != null) recommended.VersionName = string.Concat(recommended.VersionName, " Recommended");*/
                ForgeVersions = new ObservableCollection<ForgeVersion>(versions);
                HasForgeVersions = ForgeVersions.Count > 0;
                if (string.IsNullOrEmpty(pName) || pName.Contains("Forge")) PlaceholderProfileName = string.Concat("Forge ", mcVersion);
            }
            catch (Exception ex)
            {
                HasForgeVersions = false;
                Logger.Error($"There was an exception during updating forge versions for minecraft {mcVersion}", ex);
                MessageBox.Show($"An error occured during fetching forge versions.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
