using System.Windows.Forms;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM;

namespace TCM_Launcher.ViewModel.Windows
{
    public class ImportModViewModel : ViewModelBase
    {
        private readonly IProfileModService profileModService;
        public ImportModViewModel(IProfileModService profileModService)
        {
            this.profileModService = profileModService;
        }


        private string modName;

        public string ModName
        {
            get { return modName; }
            set { modName = value; OnPropertyChange(); }
        }
        private string modVersion;

        public string ModVersion
        {
            get { return modVersion; }
            set { modVersion = value; OnPropertyChange(); }
        }
        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; OnPropertyChange(); }
        }

        private string sourceFile;
        public string SourceFile
        {
            get { return sourceFile; }
            set { sourceFile = value; OnPropertyChange(); }
        }

        private bool clientSide;

        public bool ClientSide
        {
            get { return clientSide; }
            set { clientSide = value; OnPropertyChange(); }
        }

        private bool serverSide;

        public bool ServerSide
        {
            get { return serverSide; }
            set { serverSide = value; OnPropertyChange(); }
        }

        public void ImportFile()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = "Select a mod to import",
                    Filter = "Jar files (*.jar)|*.jar",
                };
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    FileName = dialog.SafeFileName;
                    SourceFile = dialog.FileName;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when importing mod.", ex);
            }
        }

        public bool ImportMod()
        {
            if(string.IsNullOrEmpty(ModName) || string.IsNullOrWhiteSpace(ModName))
            {
                Constants.MessageBoxError("Set mod name.");
                return false;
            }
            if(ModName.Length > 20)
            {
                Constants.MessageBoxError("Mod name is too long.");
                return false;
            }
            if(string.IsNullOrEmpty(ModVersion) || string.IsNullOrWhiteSpace(ModVersion))
            {
                Constants.MessageBoxError("Set mod version.");
                return false;
            }
            if(ModVersion.Length > 20)
            {
                Constants.MessageBoxError("Mod version is too long.");
                return false;
            }
            if(string.IsNullOrEmpty(FileName) || string.IsNullOrWhiteSpace(FileName))
            {
                Constants.MessageBoxError("Select a .jar file for the mod.");
                return false;
            }
            return true;
        }
    }
}
