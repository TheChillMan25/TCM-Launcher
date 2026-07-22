using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls;

namespace TCM_Launcher.View.UserControls
{
    public partial class ModSearchResultCardView : UserControl
    {
        private ModSearchResultCardViewModel viewModel => DataContext as ModSearchResultCardViewModel;
        public ModSearchResultCardView()
        {
            InitializeComponent();
        }
    }
}
