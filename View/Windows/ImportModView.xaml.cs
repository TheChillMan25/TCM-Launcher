using System.Windows;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.ViewModel.Windows;

namespace TCM_Launcher.View.Windows
{
    public partial class ImportModView : Window
    {
        public ImportModViewModel viewModel;
        public ImportModView(ImportModViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ImportFile();
        }

        private async void ImportModButton_Click(object sender, RoutedEventArgs e)
        {
            FileButton.IsEnabled = false;
            ImportModButton.IsEnabled = false;
            bool imported = viewModel.ImportMod();
            FileButton.IsEnabled = true;
            ImportModButton.IsEnabled = true;
            if (imported)
            {
                DialogResult = true;
            }
            else
            {
                DialogResult = false;
                return;
            }
            Close();
        }

        private void ClearFileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.FileName = "";
        }
    }
}
