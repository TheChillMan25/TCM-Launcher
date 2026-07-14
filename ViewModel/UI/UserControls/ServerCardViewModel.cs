using MineStatLib;
using System.Windows;
using System.Windows.Media;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;
using TCM_Launcher.View.Windows;

namespace TCM_Launcher.ViewModel.UI.UserControls
{
    public class ServerCardViewModel : ViewModelBase
    {
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
			return await ServerService.Instance.DeleteServer(Server.Id);
		}

		public async Task<bool> EditServer()
		{
			AddServerView s = new AddServerView(Server.Id, Server.Name, Server.Address, Server.MCVersion, Server.BindedProfileId);
			s.Owner = Application.Current.MainWindow;
			await s.InitializeDataAsync();
			bool edit = s.ShowDialog() ?? false;
			if (edit && s.CreatedServer != null) Server = s.CreatedServer;
			return edit;
		}

		public async Task QuickLaunchAsync()
		{
			var profile = await GameProfileService.Instance.GetProfile(Server.BindedProfileId!);
			if (profile != null) await LauncherService.Instance.LaunchProfileAsync(profile, Server.Address);
		}

		private async Task StartContinuousPingAsync()
		{
			Ping = await ServerService.Instance.FetchServerInfoAsync(Server.Address);
			UpdateColor();

			using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
			while(await timer.WaitForNextTickAsync())
			{
				Ping = await ServerService.Instance.FetchServerInfoAsync(Server.Address);
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
