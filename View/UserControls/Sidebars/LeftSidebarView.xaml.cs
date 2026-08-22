using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.Sidebars;

namespace TCM_Launcher.View.UserControls.Sidebars
{
    public partial class LeftSidebarView : UserControl
    {
        private LeftSidebarViewModel viewModel => DataContext as LeftSidebarViewModel;

        public LeftSidebarView()
        {
            InitializeComponent();
        }

        private void HomeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.ShowHome();
        }

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (viewModel != null) await viewModel.InitializeAsync();
        }

        private void SettingsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.ShowAppSettings();
        }
    }
}
