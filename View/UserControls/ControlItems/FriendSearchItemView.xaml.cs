using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.View.UserControls.ControlItems
{
    public partial class FriendSearchItemView : UserControl
    {
        private FriendSearchItemViewModel viewModel => DataContext as FriendSearchItemViewModel;
        public FriendSearchItemView()
        {
            InitializeComponent();
        }

        private async void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            AddFriendButton.IsEnabled = false;
            await viewModel.AddFriendAsync();
            AddFriendButton.IsEnabled = true;
        }
    }
}
