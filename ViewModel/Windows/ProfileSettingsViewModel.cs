using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;

namespace TCM_Launcher.ViewModel.Windows
{
	public class ProfileSettingsViewModel : ViewModelBase
    {
		private readonly IGameProfileService gameProfileService;
		private readonly IProfileSettingsService profileSettingsService;
        public ProfileSettingsViewModel(IGameProfileService gameProfileService, IProfileSettingsService profileSettingsService)
        {
			this.gameProfileService = gameProfileService;
			this.profileSettingsService = profileSettingsService;
        }

        private GameProfile gameProfile;

		public GameProfile GameProfile
		{
			get { return gameProfile; }
			set 
			{ 
				gameProfile = value;
				OnPropertyChange();
			}
		}
		
		private ProfileSettings settings;

		public ProfileSettings Settings
		{
			get { return settings; }
			set 
			{ 
				settings = value;
				OnPropertyChange();
			}
		}

		private int maxRam;

		public int MaxRam
        {
			get { return maxRam; }
			set 
			{
				maxRam = value;
				OnPropertyChange();
			}
		}

		private string jvmArgs;

		public string JVMArgs
		{
			get { return jvmArgs; }
			set 
			{
				jvmArgs = value;
				OnPropertyChange();
			}
		}

		private string title;

		public string Title
		{
			get { return title; }
			set 
			{
				title = value;
				OnPropertyChange();
			}
		}

		public async Task Initialize(GameProfile p)
		{

            GameProfile = p;
            Title = $"{GameProfile.ProfileName} : Settings";
            await LoadSettingsAsync();
        }

		public async Task SaveSettingsAsync(string pName, int ram, string jvmArgs)
		{
			GameProfile.ProfileName = pName;
			await gameProfileService.UpdateProfileAsync(GameProfile.Id, GameProfile);
			await profileSettingsService.SetProfileSettingsAsync(new ProfileSettings
			{
				GameProfileId = GameProfile.Id,
				Ram = ram,
				JVMArgs = jvmArgs
			});
		}

		public async Task LoadSettingsAsync()
        {
            Settings = await profileSettingsService.GetProfileSettings(GameProfile.Id);
			MaxRam = Settings.Ram ?? 4096;
			JVMArgs = Settings.JVMArgs ?? "";
        }
	}
}
