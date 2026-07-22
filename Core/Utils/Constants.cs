using System.IO;

namespace TCM_Launcher.Core.Utils
{
    internal static class Constants
    {
        private static readonly string launcherFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TCM Launcher");
        public static readonly string LauncherFolder = PathUtils.GetSafePath(launcherFolder);
        public static readonly string SharedPath = Path.Combine(LauncherFolder, "shared");
        public static readonly string ProfilesPath = Path.Combine(LauncherFolder, "profiles");
        public static readonly int DefaultRam = 4096;
        public static readonly string BugReportFormURL = "https://forms.gle/UxmTjoazkzG5yLBd6";
        public static readonly string AccountsJSONPath = Path.Combine(LauncherFolder, "accounts.json");
        public static readonly string ProfileModsManifest = "profile_mods.json";
    }
}
