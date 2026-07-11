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
			LoadSettings();
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

		public void SaveSettings(string pName, int ram, string jvmArgs)
		{
			GameProfile.ProfileName = pName;
			GameProfileService.Instance.UpdateProfile(GameProfile.Id, GameProfile);
			ProfileSettingsService.Instance.SetProfileSettings(new ProfileSettings
			{
				GameProfileId = GameProfile.Id,
				Ram = ram,
				JVMArgs = jvmArgs
			});
		}

		public void LoadSettings()
        {
            Settings = ProfileSettingsService.Instance.GetProfileSettings(GameProfile.Id);
			MaxRam = Settings.Ram ?? 4096;
			JVMArgs = Settings.JVMArgs ?? "";
        }
	}
}
