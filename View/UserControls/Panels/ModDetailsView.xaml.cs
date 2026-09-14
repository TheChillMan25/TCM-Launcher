using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class ModDetailsView : UserControl
    {
        public ModDetailsViewModel viewModel => DataContext as ModDetailsViewModel;
        public ModDetailsView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ImportFile();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FileButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
            viewModel.Save();
        }

        private void ClearFileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.FileName = "";
        }
    }
}
