using TCM_Launcher.MVVM.ViewModel;

namespace TCM_Launcher.ViewModel.UserControls.Panels
{
    public class LoadingScreenViewModel : ViewModelBase
    {
		private string status;

		public string Status
		{
			get { return status; }
			set { status = value; OnPropertyChange(); }
		}

		private double progress;

		public double Progress
		{
			get { return progress; }
			set { progress = value; OnPropertyChange(); }
		}

        public Action? OnPanelCloseRequested { get; set; }
    }
}
