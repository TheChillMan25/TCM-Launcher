using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class ExportModpackView : UserControl
    {
        private ExportModpackViewModel viewModel => DataContext as ExportModpackViewModel;
        public ExportModpackView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }

        private void ToggleModExport_Click(object sender, RoutedEventArgs e)
        {
            if(sender is CheckBox cb)
            {
                bool isChecked = cb.IsChecked == true;
                string? modId = cb.Tag as string;

                viewModel.ToggleModExport(modId, isChecked);
            }
        }

        private void SelectAll_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb)
            {
                bool isChecked = cb.IsChecked == true;

                viewModel.ToggleAllModsExport(isChecked);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ExportButton.IsEnabled = false;
            await viewModel.ExportModpackAsync();
            ExportButton.IsEnabled = true;
        }

        private void CloseSharePanelButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SharePanelVisible = false;
        }

        private void ShowSharePanel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SharePanelVisible = true;
        }
    }
}
