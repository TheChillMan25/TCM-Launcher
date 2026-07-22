using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls;

namespace TCM_Launcher.View.UserControls
{
    public partial class AddContentView : UserControl
    {
        private AddContentViewModel viewModel => DataContext as AddContentViewModel;
        public AddContentView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.Close();
        }
    }
}
