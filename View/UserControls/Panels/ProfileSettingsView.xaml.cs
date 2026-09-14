using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.Model.DB;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class ProfileSettingsView : UserControl
    {
        private ProfileSettingsViewModel viewModel => DataContext as ProfileSettingsViewModel;
        public ProfileSettingsView()
        {
            InitializeComponent();
        }

        public async Task Initialize(GameProfile p)
        {
            await viewModel.Initialize(p);
            if (viewModel.Settings == null) viewModel.Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string pName = ProfileNameInput.Text;
            int ram = (int)RamSlider.Value;
            string jvmArgs = JVMArgInput.Text;
            await viewModel.SaveSettingsAsync(pName, ram, jvmArgs);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }
    }
}
