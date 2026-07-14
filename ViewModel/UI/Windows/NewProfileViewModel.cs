using System.Collections.ObjectModel;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Model.DB.Versions;
using TCM_Launcher.Model.UI.Forge;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;

namespace TCM_Launcher.ViewModel.UI.Windows
{
    internal class NewProfileViewModel : ViewModelBase
    {
        public NewProfileViewModel()
        {
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
            if (VersionsService.Instance.SyncTask != null)
            {
                await VersionsService.Instance.SyncTask;
            }
            var loadedVersions = await VersionsService.Instance.GetVanillaVersions();

            Versions = new ObservableCollection<VanillaVersion>(loadedVersions);
            ForgeVersions = new();
        }

        public async Task UpdateForgeVersions(string mcVersion, string pName = "")
        {
            try
            {
                var versions = await VersionsService.Instance.GetForgeVersions(mcVersion);
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
