using TCM_Launcher.MVVM.ViewModel;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class FriendViewModel : ViewModelBase
    {
        public Func<string, Task>? OnRemoveFriendRequested { get; set; }
        public string UUID { get; set; }
        public string Username { get; set; }

        public void RemoveFriend()
        {
            OnRemoveFriendRequested?.Invoke(UUID);
        }
    }
}
