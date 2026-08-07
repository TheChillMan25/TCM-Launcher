using System.Windows.Threading;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using static TCM_Launcher.Core.Utils.Constants;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class AppSettingsViewModel : ViewModelBase
    {
        private readonly IAppSettingsService appSettingsService;

        public AppSettingsViewModel(IAppSettingsService appSettingsService)
        {
            this.appSettingsService = appSettingsService;
        }

        private bool isLoading;
        private AppSettings settings;

        private int maxP;
        public int MaximumParalellDownloads
        {
            get { return maxP; }
            set 
            { 
                if (maxP == value) return;
                maxP = Math.Min(value, 10); 
                OnPropertyChange();
                _ = SaveSettings();
            }
        }

        private bool startWithWindows;
        public bool StartWithWindows
        {
            get { return startWithWindows; }
            set 
            { 
                if (startWithWindows == value) return;
                startWithWindows = value; 
                OnPropertyChange();
                _ = SaveSettings();
            }
        }

        private LauncherWindowBehaviour closeButtonBehaviour;
        public LauncherWindowBehaviour CloseButtonBehaviour
        {
            get { return closeButtonBehaviour; }
            set 
            {
                if (closeButtonBehaviour == value) return;
                closeButtonBehaviour = value; 
                OnPropertyChange();
                _ = SaveSettings();
            }
        }

        private LauncherWindowBehaviour onGameStart;
        public LauncherWindowBehaviour OnGameStart
        {
            get { return onGameStart; }
            set 
            {
                if (onGameStart == value) return;
                onGameStart = value;
                OnPropertyChange();
                _ = SaveSettings();
            }
        }

        public IEnumerable<LauncherWindowBehaviour> CloseButtonBehaviours 
            => Enum.GetValues(typeof(LauncherWindowBehaviour))
            .Cast<LauncherWindowBehaviour>().Where(b => b != LauncherWindowBehaviour.KeepOpen);

        public IEnumerable<LauncherWindowBehaviour> OnGameStartBehaviours
            => Enum.GetValues(typeof(LauncherWindowBehaviour)).Cast<LauncherWindowBehaviour>();

        public async Task OnLoaded()
        {
            isLoading = true;
            try
            {
                if (appSettingsService.LoadTask != null)
                {
                    await appSettingsService.LoadTask;
                }

                settings = appSettingsService.AppSettings;

                StartWithWindows = settings.StartWithWindows;
                CloseButtonBehaviour = settings.CloseButtonBehaviour;
                OnGameStart = settings.OnGameStart;
                MaximumParalellDownloads = settings.MaximumParalellDownloads;
            }
            finally
            {
                isLoading = false;
            }
        }

        public async Task SaveSettings()
        {
            if (isLoading || settings == null) return;
            settings.StartWithWindows = StartWithWindows;
            settings.CloseButtonBehaviour = CloseButtonBehaviour;
            settings.OnGameStart = OnGameStart;
            settings.MaximumParalellDownloads = MaximumParalellDownloads;

            await appSettingsService.SaveSettings(settings);
        }
    }
}
