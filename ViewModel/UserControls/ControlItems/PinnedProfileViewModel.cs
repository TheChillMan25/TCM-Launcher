using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class PinnedProfileViewModel : ViewModelBase
    {
        private IGameProfileService gameProfileService;
        public PinnedProfileViewModel(IGameProfileService gameProfileService)
        {
            this.gameProfileService = gameProfileService;
        }

        private GameProfile profile;
		public GameProfile Profile
		{
			get { return profile; }
			set { profile = value; OnPropertyChange(); }
		}

        private bool playEnabled = true;

        public bool PlayEnabled
        {
            get { return playEnabled; }
            set { playEnabled = value; OnPropertyChange(); }
        }


        public Action<string> OnQuickPlayRequested { get; set; }
        public Action<GameProfile> OnUnPinRequested { get; set; }

        public void QuickPlay()
        {
            OnQuickPlayRequested.Invoke(Profile.Id);
        }

        public async Task UnPinProfile()
        {
            OnUnPinRequested.Invoke(Profile);
        }

        public void UpdateProfileName(string name)
        {
            Profile.ProfileName = name;
            OnPropertyChange(nameof(Profile));
        }
    }
}