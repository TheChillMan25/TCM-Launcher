using TCM_Launcher.MVVM;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class ModSearchResultCardViewModel : ViewModelBase
    {
        private ModSearchResult mod;
		public ModSearchResult Mod
		{
			get { return mod; }
			set 
			{ 
				mod = value; 
				OnPropertyChange(); 
			
			}
		}
	}
}
