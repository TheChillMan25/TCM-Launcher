using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.View.UserControls.ControlItems
{
    public partial class ProfileModView : UserControl
    {
        private ProfileModViewModel viewModel => DataContext as ProfileModViewModel;
        public ProfileModView()
        {
            InitializeComponent();
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.RemoveModFromProfile();
        }

        private void ImportFileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ImportFile();
        }
    }
}
