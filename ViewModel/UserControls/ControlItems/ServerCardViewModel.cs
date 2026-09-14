using Microsoft.Extensions.DependencyInjection;
using MineStatLib;
using System.Windows;
using System.Windows.Media;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM.ViewModel;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class ServerCardViewModel : ViewModelBase
    {
		private readonly IServerService serverService;
		private readonly IGameProfileService gameProfileService;
		private readonly ILauncherService launcherService;
		private readonly IOverlayService overlayService;
        public ServerCardViewModel(IServerService serverService, IGameProfileService gameProfileService, ILauncherService launcherService, IOverlayService overlayService)
        {
            this.serverService = serverService;
            this.gameProfileService = gameProfileService;
            this.launcherService = launcherService;
            this.overlayService = overlayService;
        }

        public Action<Server>? OnDeleteRequested { get; set; }
        public Action<Server>? OnUpdateRequested { get; set; }
        public Action<string, Server>? OnQuickJoinRequested { get; set; }

        private Server server;
		public Server Server
		{
			get { return server; }
			set 
			{ 
				server = value;
				OnPropertyChange();
				OnPropertyChange(nameof(HasBindedProfile));
				if(server != null)
				{
					_ = StartContinuousPingAsync();
				}
			}
		}
		public bool HasBindedProfile => Server.BindedProfileId != null;

		private MineStat ping;

		public MineStat Ping

        {
			get { return ping; }
			set { ping = value; }
		}

		private bool playButtonIsEnabled = true;

		public bool PlayButtonIsEnabled
		{
			get { return playButtonIsEnabled; }
			set { playButtonIsEnabled = value; OnPropertyChange(); }
		}

		private SolidColorBrush color = Brushes.Red;

		public SolidColorBrush Color
		{
			get { return color; }
			set 
			{
				color = value;
				OnPropertyChange();
			}
		}

		public async Task DeleteServer()
		{
			bool success = await serverService.DeleteServer(Server.Id);
			if (success) OnDeleteRequested?.Invoke(Server);
		}

		public async Task EditServer()
		{
			var result = await overlayService.ShowServerPanelAsync(Server);
			if (result != null)
			{
                Server = result;
				OnUpdateRequested?.Invoke(Server);
            }
        }

		public async Task QuickLaunchAsync()
		{
			var profile = await gameProfileService.GetProfileAsync(Server.BindedProfileId!);
			if (profile != null)
			{
                PlayButtonIsEnabled = false;
				OnQuickJoinRequested?.Invoke(profile.Id, Server);
            }
		}

		private async Task StartContinuousPingAsync()
		{
			Ping = await serverService.FetchServerInfoAsync(Server.Address);
			UpdateColor();

			using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
			while(await timer.WaitForNextTickAsync())
			{
				Ping = await serverService.FetchServerInfoAsync(Server.Address);
				UpdateColor();
            }
		}

		private void UpdateColor()
		{
            if (Ping != null && Ping.ServerUp) Color = Brushes.LightGreen;
            else Color = Brushes.Red;
        }

        public void UnbindProfile()
        {
			Server.BindedProfile = null;
			Server.BindedProfileId = null;
			OnPropertyChange(nameof(HasBindedProfile));
        }
    }
}
