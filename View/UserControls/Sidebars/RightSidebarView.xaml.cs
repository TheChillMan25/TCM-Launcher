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

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await viewModel.OnLoaded();
        }

        private async void AddServerButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await viewModel.OpenAddServerWindowAsync();
        }

        private void BugReportButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.Bugreport();
        }
    }
}
