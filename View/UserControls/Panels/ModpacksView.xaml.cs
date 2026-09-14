using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class ModpacksView : UserControl
    {
        private ModpacksViewModel viewModel => DataContext as ModpacksViewModel;
        public ModpacksView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }
        private void CloseSharePanelButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CloseSharePanel();
        }

        private async void ShareModpackButton_Click(object sender, RoutedEventArgs e)
        {
            ShareModpackButton.IsEnabled = false;
            await viewModel.ShareModpackAsync();
        }
    }
}
