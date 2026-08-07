using CmlLib.Core.Auth;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.View.Windows;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.ViewModel.UserControls.Sidebars
{
    public class RightSidebarViewModel : ViewModelBase
    {
        private readonly IMicrosoftService microsoftService;
        private readonly IServerService serverService;
        public RightSidebarViewModel(IMicrosoftService microsoftService, IServerService serverService)
        {
            this.microsoftService = microsoftService;
            this.serverService = serverService;
        }

        public Func<string, Server, Task> OnQuickJoinRequested { get; set; }

        private string username;

        public string Username
        {
            get { return username; }
            set { username = value; OnPropertyChange(); }
        }

        private string offlineUsername = "User123";
        public bool IsLoggedIn => MSession != null;

        private MSession mSession;
        public MSession MSession
        {
            get { return mSession; }
            set 
            {
                mSession = value;
                OnPropertyChange();
                OnPropertyChange(nameof(IsLoggedIn));
                UpdateIcon();
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


        private Geometry buttonIcon;
        public Geometry ButtonIcon
        {
            get { return buttonIcon; }
            set { buttonIcon = value; OnPropertyChange(); }
        }

        private void UpdateIcon()
        {
            string key = IsLoggedIn ? "LogoutIcon" : "MicrosoftIcon";

            if (App.Current != null)
            {
                ButtonIcon = App.Current.FindResource(key) as Geometry;
            }
        }

        public async Task MicrosoftLoginAsync(bool silent = false)
        {
            MSession = await microsoftService.MicrosoftLoginAsync(silent);
            if (MSession != null && !string.IsNullOrEmpty(MSession.Username))
            {
                Username = MSession.Username;
            }
        }
        public async Task MicrosoftLogoutAsync()
        {
            bool success = await microsoftService.MicrosoftSignOutAsync();
            if (success)
            {
                MSession = null;
                Username = offlineUsername;
                OnPropertyChange(nameof(IsLoggedIn));
            }
        }
        public async Task OnUserSessionButtonClick()
        {
            try
            {
                if (IsLoggedIn)
                {
                    await MicrosoftLogoutAsync();
                }
                else
                {
                    await MicrosoftLoginAsync();
                }
            }
            catch
            {

            }
        }
        public async Task OnLoaded()
        {
            await MicrosoftLoginAsync(true);
            if (MSession == null) Username = offlineUsername;
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

        public async Task OpenAddServerWindowAsync()
        {
            var s = App.ServiceProvider.GetRequiredService<AddServerView>();
            await s.InitializeDataAsync();
            s.Owner = Application.Current.MainWindow;
            s.Owner.Opacity = 0.4;
            bool create = s.ShowDialog() ?? false;
            if (s.CreatedServer != null)
            {
                var cardVM = App.ServiceProvider.GetRequiredService<ServerCardViewModel>();
                cardVM.Server = s.CreatedServer;
                cardVM.OnDeleteRequested = DeleteServer;
                cardVM.OnUpdateRequested = UpdateServer;
                cardVM.OnQuickJoinRequested = QuickJoin;
                Servers.Add(cardVM);
            }
            s.Owner.Opacity = 1;
        }

        private void QuickJoin(string profileId, Server server)
        {
            OnQuickJoinRequested?.Invoke(profileId, server);
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
            var s = Servers.FirstOrDefault(s => s.Server.BindedProfileId == profileId);
            if (s != null)
            {
                s.PlayButtonIsEnabled = value;
            }
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

        public void DeleteProfile(string profileId)
        {
            var existing = Servers.FirstOrDefault(s => s.Server.BindedProfileId == profileId);
            if(existing != null)
            {
                existing.UnbindProfile();
            }
        }
    }
}
