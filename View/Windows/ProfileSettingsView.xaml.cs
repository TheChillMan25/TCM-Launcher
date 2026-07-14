using System.Windows;
using TCM_Launcher.Model.DB;
using TCM_Launcher.ViewModel.UI.Windows;

namespace TCM_Launcher.View.Windows
{
    public partial class ProfileSettingsView : Window
    {
        ProfileSettingsViewModel viewModel;
        public ProfileSettingsView(GameProfile p)
        {
            InitializeComponent();
            viewModel = new ProfileSettingsViewModel(p);
            DataContext = viewModel;
            if (viewModel.Settings == null) Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string pName = ProfileNameInput.Text;
            int ram = (int)RamSlider.Value;
            string jvmArgs = JVMArgInput.Text;
            await viewModel.SaveSettingsAsync(pName, ram, jvmArgs);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
