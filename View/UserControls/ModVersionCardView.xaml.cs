using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls;

namespace TCM_Launcher.View.UserControls
{
    public partial class ModVersionCardView : UserControl
    {
        private ModVersionCardViewModel viewModel => DataContext as ModVersionCardViewModel;
        public ModVersionCardView()
        {
            InitializeComponent();
        }

        private void DownloadButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.AddToProfile();
        }
    }
}
