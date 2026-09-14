using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.View.UserControls.ControlItems
{
    public partial class FriendView : UserControl
    {
        private FriendViewModel viewModel => DataContext as FriendViewModel;
        public FriendView()
        {
            InitializeComponent();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.VerticalOffset = 5;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void RemoveFriend_Click(object sender, RoutedEventArgs e)
        {
            OptionsButton.IsEnabled = false;
            viewModel.RemoveFriend();
            OptionsButton.IsEnabled = false;
        }
    }
}
