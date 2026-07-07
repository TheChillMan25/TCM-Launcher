using System.ComponentModel.DataAnnotations;

namespace TCM_Launcher.Model.DB.Versions
{
    public class VersionBase
    {
        [Key]
        public string VersionName { get; set; }
    }
}
