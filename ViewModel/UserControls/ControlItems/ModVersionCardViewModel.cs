using System.Windows.Media;
using TCM_Launcher.Interfaces;
using TCM_Launcher.MVVM;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class ModVersionCardViewModel : ViewModelBase
    {
        private readonly IProfileModService profileModService;
        public ModVersionCardViewModel(IProfileModService profileModService)
        {
            this.profileModService = profileModService;
        }

        public string ProfileId { get; set; }

        public Func<ModVersion, Task>? OnAddToProfileRequested;

        private ModVersion version;
        public ModVersion Version
        {
            get { return version; }
            set 
            { 
                version = value; 
                OnPropertyChange();
                SetReleaseType();
            }
        }

        private string releaseType;
        public string ReleaseType
        {
            get { return releaseType; }
            set 
            { 
                releaseType = value;
                OnPropertyChange();
                SetReleaseColor();
            }
        }

        private bool isBindedToProfile;

        public bool IsBindedToProfile
        {
            get { return isBindedToProfile; }
            set { isBindedToProfile = value; OnPropertyChange(); }
        }

        private SolidColorBrush releaseTypeColor;
        public SolidColorBrush ReleaseTypeColor
        {
            get { return releaseTypeColor; }
            set 
            { 
                releaseTypeColor = value;
                OnPropertyChange();
            }
        }
        public void SetReleaseType()
        {
            switch (Version.VersionType)
            {
                case "release":
                    ReleaseType = "R";
                    break;
                case "beta":
                    ReleaseType = "B";
                    break;
                case "alpha":
                    ReleaseType = "A";
                    break;
            }
        }
        private void SetReleaseColor()
        {
            switch (Version.VersionType)
            {
                case "release":
                    ReleaseTypeColor = Brushes.Teal; 
                    break;
                case "beta":
                    ReleaseTypeColor = Brushes.Orange;
                    break;
                case "alpha":
                    ReleaseTypeColor = Brushes.Red;
                    break;
            }
        }

        public void AddToProfile()
        {
            OnAddToProfileRequested.Invoke(Version);
        }

        public async Task SetIsBindedToProfile()
        {
            IsBindedToProfile = await profileModService.IsVersionBindedToProfileAsync(ProfileId, Version);
        }
    }
}
