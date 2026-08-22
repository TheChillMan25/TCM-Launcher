using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCM_Launcher.ViewModel.Windows;

namespace TCM_Launcher.View.Windows
{
    public partial class ExportModpackView : Window
    {
        private ExportModpackViewModel viewModel;
        public ExportModpackView(ExportModpackViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }

        public void Initialize(string profileId, string profileName, List<ProfileModViewModel> mods)
        {
            if(viewModel != null)
            {
                viewModel.ProfileId = profileId;
                viewModel.ProfileName = profileName;
                viewModel.ExportableMods = new(mods);
            }
        } 

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
            if (viewModel != null) await viewModel.ExportModpack();
            DialogResult = true;
        }
    }
}
