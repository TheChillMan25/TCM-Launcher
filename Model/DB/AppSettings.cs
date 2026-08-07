using static TCM_Launcher.Core.Utils.Constants;

namespace TCM_Launcher.Model.DB
{
    public class AppSettings
    {
        public int Id { get; set; }
        public bool StartWithWindows { get; set; }
        public LauncherWindowBehaviour CloseButtonBehaviour { get; set; } = LauncherWindowBehaviour.Minimize;
        public LauncherWindowBehaviour OnGameStart { get; set; } = LauncherWindowBehaviour.Minimize;
        public int MaximumParalellDownloads { get; set; } = 2;
    }
}
