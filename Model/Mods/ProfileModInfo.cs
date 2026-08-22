using TCML_Class_library;

namespace TCM_Launcher.Model.Mods
{
    public class ProfileModInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public ModSource Source { get; set; }
        public string FileName { get; set; }
        public string DownloadUrl { get; set; }
        public string IconUrl { get; set; }
        public string Author { get; set; }
        public string Client_Side { get; set; }
        public string Server_Side { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class ProfileModpackManifest
    {
        public string ProfileName { get; set; }
        public ModpackDependency Dependencies { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<ProfileModInfo> Mods { get; set; } = new List<ProfileModInfo>();
    }

    public class ModpackDependency
    {
        public string MinecraftVersion { get; set; }
        public string ForgeVersion { get; set; }
    }
}
