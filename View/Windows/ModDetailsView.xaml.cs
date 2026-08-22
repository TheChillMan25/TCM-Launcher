using System.Windows;
using TCM_Launcher.ViewModel.Windows;

namespace TCM_Launcher.View.Windows
{
    public partial class ModDetailsView : Window
    {
        public ModDetailsViewModel viewModel;
        public ModDetailsView(ModDetailsViewModel viewModel)
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FileButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
            bool imported = viewModel.Save();
            FileButton.IsEnabled = true;
            SaveButton.IsEnabled = true;
            if (imported)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                return;
            }
        }

        private void ClearFileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.FileName = "";
        }
    }
}
