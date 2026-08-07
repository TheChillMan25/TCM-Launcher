using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls;

namespace TCM_Launcher.View.UserControls
{
    public partial class AppSettingsView : UserControl
    {
        private AppSettingsViewModel viewModel => DataContext as AppSettingsViewModel;
        public AppSettingsView()
        {
            InitializeComponent();
        }

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (viewModel != null) await viewModel.OnLoaded();
        }
    }
}
