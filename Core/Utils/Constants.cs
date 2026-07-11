using System.IO;

namespace TCM_Launcher.Core.Utils
{
    internal static class Constants
    {
        private static string launcherFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TCM Launcher");
        public static readonly string LauncherFolder = PathUtils.GetSafePath(launcherFolder);
        public static readonly string SharedPath = Path.Combine(LauncherFolder, "shared");
        public static readonly string ProfilesPath = Path.Combine(LauncherFolder, "profiles");
        public static readonly int DefaultRam = 4096;
    }
}
