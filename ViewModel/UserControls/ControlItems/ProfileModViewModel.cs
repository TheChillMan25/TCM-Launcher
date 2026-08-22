using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class ProfileModViewModel : ViewModelBase
    {
        public ProfileModViewModel()
        {

        }

        public Func<string, Task>? OnModRemoveRequested { get; set; }
        public Func<ProfileModViewModel, Task>? OnModSettingsRequested { get; set; }
        public Func<ProfileModInfo, bool, Task> OnToggleModRequested { get; set; }

        private ProfileModInfo mod;

		public ProfileModInfo Mod
		{
			get { return mod; }
			set 
            {
                mod = value; 
                OnPropertyChange(); 
                IconUrl = Mod.IconUrl;
                if (mod.Source == ModSource.Imported) mod.Author = "IMPORTED";
            }
		}

        private bool missingJar;

        public bool MissingJar
        {
            get { return missingJar; }
            set { missingJar = value; OnPropertyChange(); }
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

        private bool modEnabled;

        public bool ModEnabled
        {
            get { return modEnabled; }
            set 
            {
                modEnabled = value; 
                OnPropertyChange();
            }
        }

        public async Task RemoveModFromProfile()
        {
            OnModRemoveRequested?.Invoke(Mod.Id);
        }

        public void OpenModSettings()
        {
            OnModSettingsRequested?.Invoke(this);
        }

        public void ToggleMod(bool value)
        {
            Mod.IsEnabled = value;
            ModEnabled = value;
            OnToggleModRequested?.Invoke(Mod, Mod.IsEnabled);
        }
    }
}
