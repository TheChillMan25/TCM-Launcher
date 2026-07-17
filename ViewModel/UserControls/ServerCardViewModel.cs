using Microsoft.Extensions.DependencyInjection;
using MineStatLib;
using System.Windows;
using System.Windows.Media;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;
using TCM_Launcher.View.Windows;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class ServerCardViewModel : ViewModelBase
    {
		private readonly IServerService serverService;
		private readonly IGameProfileService gameProfileService;
		private readonly ILauncherService launcherService;
        public ServerCardViewModel(IServerService serverService, IGameProfileService gameProfileService, ILauncherService launcherService)
        {
            this.serverService = serverService;
            this.gameProfileService = gameProfileService;
            this.launcherService = launcherService;
        }

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

		public async Task<bool> DeleteServer()
		{
			return await serverService.DeleteServer(Server.Id);
		}

		public async Task<bool> EditServer()
		{
			var s = App.ServiceProvider.GetRequiredService<AddServerView>();
			s.Initialize(Server.Id, Server.Name, Server.Address, Server.MCVersion, Server.BindedProfileId);
			s.Owner = Application.Current.MainWindow;
			await s.InitializeDataAsync();
			bool edit = s.ShowDialog() ?? false;
			if (edit && s.CreatedServer != null) Server = s.CreatedServer;
			return edit;
		}

		public async Task QuickLaunchAsync()
		{
			var profile = await gameProfileService.GetProfile(Server.BindedProfileId!);
			if (profile != null) await launcherService.LaunchProfileAsync(profile, Server.Address);
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
	}
}
