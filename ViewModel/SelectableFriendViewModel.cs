using TCM_Launcher.MVVM.ViewModel;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel
{
    public class SelectableFriendViewModel : ViewModelBase
    {
        public FirestoreUser User { get; }
        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; OnPropertyChange(); }
        }

        public SelectableFriendViewModel(FirestoreUser user)
        {
            User = user;
        }
    }
}
