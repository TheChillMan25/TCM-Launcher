using TCM_Launcher.Model.DB;
using static TCM_Launcher.Core.Utils.Constants;

namespace TCM_Launcher.Interfaces
{
    public interface IAppSettingsService
    {
        public AppSettings AppSettings { get; }
        Task LoadTask { get; }

        Task SaveSettings(AppSettings settings);
        void StartLoadingSettings();
        void LauncherWindowBehaviour(LauncherWindowBehaviour behavior, bool? value = null);
    }
}
