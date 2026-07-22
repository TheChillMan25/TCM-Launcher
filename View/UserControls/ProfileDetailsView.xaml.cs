using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls;

namespace TCM_Launcher.View.UserControls
{
    public partial class ProfileDetailsView : UserControl
    {
        private ProfileDetailsViewModel? viewModel => DataContext as ProfileDetailsViewModel;
        public ProfileDetailsView()
        {
            InitializeComponent();
        }
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedGameProfile == null)
            {
                MessageBox.Show("Select a profile to START.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (viewModel.SelectedGameProfile.Installed != true)
            {
                MessageBox.Show("Profile is installing files. Please wait", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await viewModel.StartGame();
        }

        private async void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.OpenProfileSettings();
        }

        private void OtherButton_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.VerticalOffset = 5;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OpenProfileFolder();
        }

        private async void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.DeleteProfileAsync();
        }

        private void BrowseContentButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ShowContentBorwser();
        }

        //private void ExportModpack_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Modpack exportalasa...");
        //}
    }
}
