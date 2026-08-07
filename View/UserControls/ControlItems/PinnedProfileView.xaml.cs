using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.View.UserControls.ControlItems
{

    public partial class PinnedProfileView : UserControl
    {
        private PinnedProfileViewModel viewModel => DataContext as PinnedProfileViewModel;
        public PinnedProfileView()
        {
            InitializeComponent();
        }

        private void QuickPlayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.QuickPlay();
        }

        private async void UnpinButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await viewModel.UnPinProfile();
        }
    }
}
