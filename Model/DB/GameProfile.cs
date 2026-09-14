using System.ComponentModel.DataAnnotations.Schema;
using TCML_Class_library;

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
        public uint? PackReleaseNumber { get; set; }
        public string? ModpackId { get; set; }
        public FirestoreModpack? Modpack { get; set; }
        public double? PlayTime { get; set; }
        [NotMapped]
        public bool IsPlaying { get; set; } = false;
    }
}
