using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class ProfileModViewModel : ViewModelBase
    {
		private readonly IProfileModService profileModService;

        public ProfileModViewModel(IProfileModService profileModService)
        {
            this.profileModService = profileModService;
        }

        public Func<string, Task>? OnModRemoveRequested { get; set; }

        private ProfileModInfo mod;

		public ProfileModInfo Mod
		{
			get { return mod; }
			set { mod = value; OnPropertyChange(); IconUrl = Mod.IconUrl; }
		}


        private string iconUrl;
        public string IconUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(iconUrl) || iconUrl.Trim() == "#")
                    return null;

                return iconUrl;
            }
            set
            {
                iconUrl = value;
                OnPropertyChange();
            }
        }

        public async Task RemoveModFromProfile()
        {
            OnModRemoveRequested.Invoke(Mod.Id);
        }

	}
}
