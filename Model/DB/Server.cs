using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCM_Launcher.Model.DB
{
    public class Server
    {
        [Key]
        public string Id { get; set; } = new Guid().ToString();
        public string Name { get; set; }
        public string MCVersion { get; set; }
        public string Address { get; set; }
        public string? BindedProfileId { get; set; }
        public GameProfile? BindedProfile { get; set; }
    }
}
