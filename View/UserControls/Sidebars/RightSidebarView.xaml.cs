using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.Sidebars;

namespace TCM_Launcher.View.UserControls.Sidebars
{
    public partial class RightSidebarView : UserControl
    {
        private RightSidebarViewModel viewModel => DataContext as RightSidebarViewModel;
        public RightSidebarView()
        {
            InitializeComponent();
        }

        private async void UserSessionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserSessionButton.IsEnabled = false;
            await viewModel.OnUserSessionButtonClick();
            UserSessionButton.IsEnabled = true;
        }

        private async void AddFriendButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.ShowFriendSearch();
        }

        private void NotificationsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.ShowNotifications();
        }

        private async void ModpacksButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await viewModel.ShowModpacksAsync();
        }
    }
}
