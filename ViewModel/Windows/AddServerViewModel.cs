using System.Collections.ObjectModel;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;

namespace TCM_Launcher.ViewModel.Windows
{
    public class AddServerViewModel : ViewModelBase
    {
        private readonly IGameProfileService gameProfileService;
        private readonly IVersionService versionService;
        private readonly IServerService serverService;

        public AddServerViewModel(IGameProfileService gameProfileService, IVersionService versionService, IServerService serverService)
        {
            this.gameProfileService = gameProfileService;
            this.versionService = versionService;
            this.serverService = serverService;
        }

        private bool Edit;
        private string ServerId;

        private string buttonText = "Add server";

        public string ButtonText
        {
            get { return buttonText; }
            set 
            {
                buttonText = value;
                OnPropertyChange();
            }
        }


        private ObservableCollection<GameProfile> profiles = new ObservableCollection<GameProfile>();

		public ObservableCollection<GameProfile> Profiles
		{
			get { return profiles; }
			set 
			{ 
				profiles = value;
				OnPropertyChange();
                OnPropertyChange(nameof(HasProfiles));
			}
		}
        public bool HasProfiles => Profiles.Count > 0;

		private ObservableCollection<string> mcVersions;

		public ObservableCollection<string> MCVersions
		{
			get { return mcVersions; }
			set 
			{
				mcVersions = value;
                OnPropertyChange();
            }
		}

		private string serverName;

		public string ServerName
		{
			get { return serverName; }
			set 
			{
				serverName = value;
				OnPropertyChange();
			}
		}

        private string serverAddress;

        public string ServerAddress
        {
            get { return serverAddress; }
            set
            {
                serverAddress = value;
                OnPropertyChange();
            }
        }
        private string serverVersion;
        public string ServerVersion
        {
            get { return serverVersion; }
            set
            {
                serverVersion = value;
                OnPropertyChange();
            }
        }

        private string bindedProfileId;

        public string BindedProfileId
        {
            get { return bindedProfileId; }
            set { bindedProfileId = value; }
        }


        private GameProfile bindedProfile;

        public GameProfile BindedProfile
        {
            get { return bindedProfile; }
            set 
            { 
                bindedProfile = value;
                OnPropertyChange();
            }
        }

        public void Initialize(string? serverId = null, string? serverName = null, string? serverAddress = null, string? serverVersion = null, string? bindedProfileId = null)
        {
            if (serverName != null || serverAddress != null || serverVersion != null) Edit = true;
            if (Edit) ButtonText = "Save";
            ServerId = serverId;
            ServerName = serverName;
            ServerAddress = serverAddress;
            ServerVersion = serverVersion;
            BindedProfileId = bindedProfileId;
        }

        public async Task InitializeDataAsync()
		{
			var mcVersions = await versionService.GetVanillaVersions();
			MCVersions = new ObservableCollection<string>(mcVersions.Select(v => v.VersionName));
		}

		public async Task<Server?> AddServer()
		{
            if (Edit) return await serverService.UpdateServer(new Server
            {
                Id = ServerId,
                Address = ServerAddress,
                Name = ServerName,
                MCVersion = ServerVersion,
                BindedProfileId = BindedProfile?.Id,
            });
            return await serverService.AddServer(ServerName, ServerAddress, ServerVersion, BindedProfile?.Id);
		}

        public async Task OnServerIPChanged()
        {
            if (!string.IsNullOrWhiteSpace(ServerAddress) && NetworkUtil.IsValidServerAddress(ServerAddress))
            {
                var sInfo = await serverService.FetchServerInfoAsync(ServerAddress);
                if (sInfo != null && sInfo.ServerUp && !string.IsNullOrWhiteSpace(sInfo.Version))
                {
                    ServerVersion = sInfo.Version;
                }
            }
        }

        public async Task LoadProfiles()
        {
            var profiles = await gameProfileService.GetProfilesWithVersionAsync(ServerVersion);
            Profiles = new ObservableCollection<GameProfile>(profiles);

            if(BindedProfileId != null)
            {
                BindedProfile = Profiles.FirstOrDefault(p => p.Id == BindedProfileId);
            }
        }
    }
}
