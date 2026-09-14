using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class LoadingScreenView : UserControl
    {
        private LoadingScreenViewModel viewModel => DataContext as LoadingScreenViewModel;
        public LoadingScreenView()
        {
            InitializeComponent();
        }
    }
}
