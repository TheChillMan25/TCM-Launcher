using System.IO;

namespace TCM_Launcher.Core
{
    internal static class Constants
    {
        public static string LauncherFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TCM Launcher");
        public static string SharedPath = Path.Combine(LauncherFolder, "Shared");
        public static string ProfilesPath = Path.Combine(LauncherFolder, "Profiles");
        public static int DefaultRam = 4096;
    }
}
