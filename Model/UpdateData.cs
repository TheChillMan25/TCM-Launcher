using Onova;

namespace TCM_Launcher.Model
{
    public class UpdateData
    {
        public UpdateManager Manager { get; set; }
        public bool CanUpdate { get; set; }
        public bool IsUpdating { get; set; }
        public Version Version { get; set; }
    }
}
