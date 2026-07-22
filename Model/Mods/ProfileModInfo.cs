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
        public bool IsEnabled { get; set; }
    }

    public class ProfileModpackManifest
    {
        public string ProfileId { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<ProfileModInfo> Mods { get; set; }
    }
}
