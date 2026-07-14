using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;

namespace TCM_Launcher.ViewModel.UI.Windows
{
	internal class ProfileSettingsViewModel : ViewModelBase
    {
        public ProfileSettingsViewModel(GameProfile p)
        {
            GameProfile = p;
			Title = $"{GameProfile.ProfileName} : Settings";
			_ = LoadSettingsAsync();
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

		public async Task SaveSettingsAsync(string pName, int ram, string jvmArgs)
		{
			GameProfile.ProfileName = pName;
			await GameProfileService.Instance.UpdateProfileAsync(GameProfile.Id, GameProfile);
			await ProfileSettingsService.Instance.SetProfileSettingsAsync(new ProfileSettings
			{
				GameProfileId = GameProfile.Id,
				Ram = ram,
				JVMArgs = jvmArgs
			});
		}

		public async Task LoadSettingsAsync()
        {
            Settings = await ProfileSettingsService.Instance.GetProfileSettings(GameProfile.Id);
			MaxRam = Settings.Ram ?? 4096;
			JVMArgs = Settings.JVMArgs ?? "";
        }
	}
}
