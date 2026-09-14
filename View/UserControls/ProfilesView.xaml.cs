using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls;

namespace TCM_Launcher.View.UserControls
{
    public partial class ProfilesView : UserControl
    {
        private ProfilesViewModel viewModel => DataContext as ProfilesViewModel;
        public ProfilesView()
        {
            InitializeComponent();
        }

        private async void AddProfileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await viewModel.OpenNewProfileWindow();
        }
    }
}
