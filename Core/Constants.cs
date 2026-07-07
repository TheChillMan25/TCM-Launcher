using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using TCM_Launcher.Model.UI;
using TCM_Launcher.Model.UI.Forge;

namespace TCM_Launcher.Core
{
    internal static class Constants
    {
        public static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TCM Launcher");
        public static string SharedPath = Path.Combine(AppDataPath, "Shared");
        public static string ProfilesPath = Path.Combine(AppDataPath, "Profiles");
        public static string SavedVersionsPath = Path.Combine(SharedPath, "version_data","vanilla_versions.json");
        public static string SavedForgeVersionsPath = Path.Combine(SharedPath, "version_data", "forge_versions.json");
        public static int DefaultRam = 4096;
    }
}
