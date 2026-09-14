using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.View.UserControls.ControlItems
{
    public partial class ModpackView : UserControl
    {
        private ModpackViewModel viewModel => DataContext as ModpackViewModel;
        public ModpackView()
        {
            InitializeComponent();
        }

        private void ShareButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.ShareModpack();
        }

        private async void CreateProfileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await viewModel.CreateProfileAsync();
        }

        private async void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await viewModel.DeleteModpackAsync();
        }
    }
}
