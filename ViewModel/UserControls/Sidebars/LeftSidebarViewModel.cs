using Microsoft.Extensions.DependencyInjection;
using Onova;
using Onova.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCM_Launcher.ViewModel.UserControls.Panels;
using static TCM_Launcher.Core.Utils.Constants;

namespace TCM_Launcher.ViewModel.UserControls.Sidebars
{
    public class LeftSidebarViewModel : ViewModelBase
    {
        private readonly IServerService serverService;
        private readonly IBackendService backendService;
        private readonly IMicrosoftService microsoftService;
        private readonly IOverlayService overlayService;
        public LeftSidebarViewModel(IServerService serverService, IBackendService backendService, IMicrosoftService microsoftService, IOverlayService overlayService)
        {
            this.serverService = serverService;
            this.backendService = backendService;
            this.microsoftService = microsoftService;
            this.overlayService = overlayService;
        }

        public Func<string, Server, Task> OnQuickJoinRequested { get; set; }
        public Action<ContentToShow>? OnHomeRequested { get; set; }
        public Action<ContentToShow>? OnAppSettingsRequested { get; set; }

        private UpdateData availableUpdate = new UpdateData();
        public UpdateData AvailableUpdate
        {
            get { return availableUpdate; }
            set
            {
                availableUpdate = value;
                OnPropertyChange();
            }
        }

        private ObservableCollection<ServerCardViewModel> servers = new ObservableCollection<ServerCardViewModel>();
        public ObservableCollection<ServerCardViewModel> Servers
        {
            get { return servers; }
            set
            {
                servers = value;
                OnPropertyChange();
            }
        }

        public string AppVersion 
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return version != null ? $"v{version?.Major}.{version?.Minor}.{version?.Build}" :
                    "v0.0.1";
            } 
        }

        public async Task InitializeAsync()
        {
            var loadProfilesTask = LoadServersAsync();
            var checkUpdatesTask = CheckForUpdatesAsync();

            await Task.WhenAll(loadProfilesTask, checkUpdatesTask);
        }


        public async Task CheckForUpdatesAsync()
        {
            #if DEBUG
                Console.WriteLine("Developer mode: Update check cancelled.");
                return;
            #endif

            try
            {
                var manager = new UpdateManager(
                    new GithubPackageResolver("TheChillMan25", "TCM-Launcher", "TCM.Launcher.zip"),
                    new ZipPackageExtractor()
                );
                AvailableUpdate.Manager = manager;

                var check = await manager.CheckForUpdatesAsync();
                if (check != null)
                {
                    AvailableUpdate.CanUpdate = check.CanUpdate;
                    OnPropertyChange(nameof(AvailableUpdate));
                    if (AvailableUpdate.CanUpdate)
                    {
                        AvailableUpdate.Version = check.LastVersion ?? check.Versions.FirstOrDefault();
                        await Update();
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exceprtion during checking for updates", ex);
            }
        }
        public async Task Update()
        {

            var update = await overlayService.ShowPopupPanelAsync("Update available", "There is an update available. Click the button to download it.", PopupAction.UPDATE);
            if (update == true)
            {
                AvailableUpdate.IsUpdating = true;
                await AvailableUpdate.Manager.PrepareUpdateAsync(AvailableUpdate.Version);
                AvailableUpdate.Manager.LaunchUpdater(AvailableUpdate.Version);
                Application.Current.Shutdown();
            }
        }
        public async Task LoadServersAsync()
        {
            var servers = await serverService.GetSavedServersAsync();
            var vmList = servers.Select(s =>
            {
                var cardVM = App.ServiceProvider.GetRequiredService<ServerCardViewModel>();
                cardVM.Server = s;
                cardVM.OnDeleteRequested = DeleteServer;
                cardVM.OnUpdateRequested = UpdateServer;
                cardVM.OnQuickJoinRequested = QuickJoin;
                return cardVM;
            }).ToList();
            Servers = new ObservableCollection<ServerCardViewModel>(vmList);
        }

        public void ShowHome()
        {
            OnHomeRequested?.Invoke(ContentToShow.Home);
        }

        public void ShowAppSettings()
        {
            OnAppSettingsRequested?.Invoke(ContentToShow.Settings);
        }

        public void Bugreport()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Constants.BugReportFormURL,
                    UseShellExecute = true
                });
                if (Directory.Exists(Path.Combine(Constants.LauncherFolder, "logs")))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        UseShellExecute = true,
                        Arguments = Path.Combine(Constants.LauncherFolder, "logs")
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception during bugreport", ex);
                MessageBox.Show("An error occured during bugreport.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public async Task OpenAddServerWindowAsync()
        {
            var result = await overlayService.ShowServerPanelAsync();
            if (result != null)
            {
                var cardVM = App.ServiceProvider.GetRequiredService<ServerCardViewModel>();
                cardVM.Server = result;
                cardVM.OnDeleteRequested = DeleteServer;
                cardVM.OnUpdateRequested = UpdateServer;
                cardVM.OnQuickJoinRequested = QuickJoin;
                Servers.Add(cardVM);
            }
        }

        public void DeleteServer(Server s)
        {
            var vmToRemove = Servers.FirstOrDefault(vm => vm.Server.Id == s.Id);
            if (vmToRemove != null) Servers.Remove(vmToRemove);
        }
        public void UpdateServer(Server s)
        {
            var sVM = Servers.FirstOrDefault(ser => ser.Server.Id == s.Id);
            if (sVM != null)
            {
                sVM.Server = s;
            }
        }

        public void ChangePlayOnServer(string profileId, bool value = true)
        {
            var servers = Servers.Where(s => s.Server.BindedProfileId == profileId);
            if (servers != null)
            {
                foreach (var s in servers)
                {
                    s.PlayButtonIsEnabled = value;
                }
            }
        }

        private void QuickJoin(string profileId, Server server)
        {
            OnQuickJoinRequested?.Invoke(profileId, server);
        }public void DeleteProfileFromServers(string profileId)
        {
            var existings = Servers.Where(s => s.Server.BindedProfileId == profileId).ToList();
            foreach (var server in existings)
            {
                server.UnbindProfile();
            }
        }
    }
}
