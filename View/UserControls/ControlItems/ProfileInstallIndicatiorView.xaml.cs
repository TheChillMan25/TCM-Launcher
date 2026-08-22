using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.View.UserControls.ControlItems
{
    public partial class ProfileInstallIndicatiorView : UserControl
    {
        private ProfileInstallIndicatorViewModel viewModel => DataContext as ProfileInstallIndicatorViewModel;
        public ProfileInstallIndicatiorView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Remove();
        }
    }
}
