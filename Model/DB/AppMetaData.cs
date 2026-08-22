using System.ComponentModel.DataAnnotations;

namespace TCM_Launcher.Model.DB
{
    public class AppMetaData
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime UpdatedAt{ get; set; }
    }
}
