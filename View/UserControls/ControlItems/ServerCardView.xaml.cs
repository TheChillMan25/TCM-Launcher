using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.View.UserControls.ControlItems
{
    public partial class ServerCardView : UserControl
    {
        private ServerCardViewModel viewModel => DataContext as ServerCardViewModel;
        public ServerCardView()
        {
            InitializeComponent();
        }

        private async void QuickStartButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.QuickLaunchAsync();
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

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.EditServer();
        }

        private async void Remove_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.DeleteServer();
        }
    }
}
