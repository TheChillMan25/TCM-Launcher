using System.IO;
using System.Windows;

namespace TCM_Launcher.Core.Utils
{
    public static class Constants
    {
        private static readonly string launcherFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TCM Launcher");
        public static readonly string LauncherFolder = PathUtils.GetSafePath(launcherFolder);
        public static readonly string SharedPath = Path.Combine(LauncherFolder, "shared");
        public static readonly string ProfilesPath = Path.Combine(LauncherFolder, "profiles");
        public static readonly int DefaultRam = 4096;
        public static readonly string BugReportFormURL = "https://forms.gle/UxmTjoazkzG5yLBd6";
        public static readonly string AccountsJSONPath = Path.Combine(LauncherFolder, "accounts.json");
        public static readonly string ProfileManifest = "profile_manifest.json";

        public enum LauncherWindowBehaviour
        {
            Minimize, Close, KeepOpen
        }
        public enum ContentToShow
        {
            Home, Settings
        }

        public static void MessageBoxError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
