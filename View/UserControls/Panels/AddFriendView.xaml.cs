using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class AddFriendView : UserControl
    {
        private SearchFriendsViewModel viewModel => DataContext as SearchFriendsViewModel;
        public AddFriendView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OnPanelCloseRequested?.Invoke();
        }
    }
}
