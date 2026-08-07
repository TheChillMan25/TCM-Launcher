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
        public Func<bool, ProfileModViewModel, Task>? OnImportFileRequested { get; set; }

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

        public async Task RemoveModFromProfile()
        {
            OnModRemoveRequested?.Invoke(Mod.Id);
        }

        public void ImportFile()
        {
            OnImportFileRequested?.Invoke(true, this);
        }
    }
}
