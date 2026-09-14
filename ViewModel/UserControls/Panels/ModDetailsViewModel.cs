using System.Windows.Forms;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM.ViewModel;

namespace TCM_Launcher.ViewModel.UserControls.Panels
{
    public class ModDetailsViewModel : ViewModelBase
    {
        public Action<(string filePath, ProfileModInfo? modInfo)?> OnPanelCloseRequested { get; set; }
        public ProfileModInfo ModDetails { get; set; }

        private string? modName;
        public string? ModName
        {
            get { return modName; }
            set { modName = value; OnPropertyChange(); }
        }

        private string? modVersion;
        public string? ModVersion
        {
            get { return modVersion; }
            set { modVersion = value; OnPropertyChange(); }
        }

        private string? fileName;
        public string? FileName
        {
            get { return fileName; }
            set { fileName = value; OnPropertyChange(); }
        }

        private string? sourceFile;
        public string? SourceFile
        {
            get { return sourceFile; }
            set { sourceFile = value; OnPropertyChange(); }
        }

        public List<string> Environments { get; set; } = new List<string>
        {
            "required", "optional", "unsupported"
        };

        private string? clientSide = "required";
        public string? ClientSide
        {
            get { return clientSide; }
            set { clientSide = value; OnPropertyChange(); }
        }

        private string? serverSide = "required";
        public string? ServerSide
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

        public void Save()
        {
            if (ModDetails == null) return;
            if(string.IsNullOrEmpty(ModName) || string.IsNullOrWhiteSpace(ModName))
            {
                Constants.MessageBoxError("Set mod name.");
                return;
            }
            if(ModName.Length > 50)
            {
                Constants.MessageBoxError("Mod name is too long.");
                return;
            }
            if(string.IsNullOrEmpty(ModVersion) || string.IsNullOrWhiteSpace(ModVersion))
            {
                Constants.MessageBoxError("Set mod version.");
                return;
            }
            if(ModVersion.Length > 50)
            {
                Constants.MessageBoxError("Mod version is too long.");
                return;
            }
            if(string.IsNullOrEmpty(FileName) || string.IsNullOrWhiteSpace(FileName))
            {
                Constants.MessageBoxError("Select a .jar file for the mod.");
                return;
            }
            if(ModDetails != null && (!string.IsNullOrEmpty(ModDetails.FileName) && ModDetails.FileName != FileName))
            {
                Constants.MessageBoxError("This .jar file doesn't belong to this mod. Select the correct .jar file.");
                return;
            }
            ModDetails.Name = ModName;
            ModDetails.Version = ModVersion;
            ModDetails.FileName = FileName;
            ModDetails.Client_Side = ClientSide;
            ModDetails.Server_Side = ServerSide;
            OnPanelCloseRequested?.Invoke((SourceFile, ModDetails));
        }

        public void Close()
        {
            OnPanelCloseRequested?.Invoke(null);
        }

        public void Initialize(ProfileModInfo? details, bool missingJar = true)
        {
            ModDetails = details ?? new();
            ModName = details?.Name;
            ModVersion = details?.Version;
            ClientSide = details?.Client_Side;
            ServerSide = details?.Server_Side;
            FileName = missingJar ? "" : details?.FileName;
        }
    }
}
