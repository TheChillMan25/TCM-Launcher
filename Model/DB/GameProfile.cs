using System.ComponentModel.DataAnnotations.Schema;

namespace TCM_Launcher.Model.DB
{
    public class GameProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? ProfileName { get; set; }
        public string? MCVersion { get; set; }
        public string? ForgeVersion { get; set; }
        public string? FileName { get; set; }
        public bool? Installed { get; set; }
        public bool? LastPlayed { get; set; }
        public bool? Pinned { get; set; }
        [NotMapped]
        public bool IsPlaying { get; set; } = false;
    }
}
