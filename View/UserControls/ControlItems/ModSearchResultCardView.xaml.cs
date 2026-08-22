using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.View.UserControls.ControlItems
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
