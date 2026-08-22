using TCM_Launcher.MVVM;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class ProfileInstallIndicatorViewModel : ViewModelBase
    {

        private double progress;

        public double Progress
        {
            get { return progress; }
            set { progress = value; OnPropertyChange(); }
        }

        private string status;

        public string Status
        {
            get { return status; }
            set { status = value; OnPropertyChange(); }
        }

        public Action? OnRemoveRequested { get; set; }

        private bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; if (value == false) OnRemoveRequested?.Invoke(); }
        }

        private string profileName;

        public string ProfileName
        {
            get { return profileName; }
            set { profileName = value; OnPropertyChange(); }
        }
        private string profileId;

        public string ProfileId
        {
            get { return profileId; }
            set { profileId = value; OnPropertyChange(); }
        }
        public void Remove()
        {
            OnRemoveRequested?.Invoke();
        }
    }
}
