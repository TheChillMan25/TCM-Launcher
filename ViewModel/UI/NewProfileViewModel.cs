using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using TCM_Launcher.Core;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.DB.Versions;
using TCM_Launcher.Model.UI;
using TCM_Launcher.Model.UI.Forge;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;

namespace TCM_Launcher.ViewModel.UI
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
            Versions = new ObservableCollection<VanillaVersion>(VersionsService.Instance.GetVanillaVersions());
            ForgeVersions = new();
        }

        public void UpdateForgeVersions(string mcVersion, string? pName = "")
        {
            try
            {
                var versions = VersionsService.Instance.GetForgeVersions(mcVersion);
                ForgeLoaderJSONData recommended = null;
                /*if (versions.Count > 0 ) recommended = versions.FirstOrDefault(v => v.IsRecommended);
                if (recommended != null) recommended.VersionName = string.Concat(recommended.VersionName, " Recommended");*/
                ForgeVersions = new ObservableCollection<ForgeVersion>(versions);
                HasForgeVersions = ForgeVersions.Count > 0;
                if(string.IsNullOrEmpty(pName)) PlaceholderProfileName = string.Concat("Forge ", mcVersion);
            }
            catch (Exception ex)
            {
                HasForgeVersions = false;
                MessageBox.Show($"An error occured during fetching forge versions: {ex}");
            }
        }
    }
}
