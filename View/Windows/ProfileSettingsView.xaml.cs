using System.Windows;
using TCM_Launcher.Model.DB;
using TCM_Launcher.ViewModel.Windows;

namespace TCM_Launcher.View.Windows
{
    public partial class ProfileSettingsView : Window
    {
        private readonly ProfileSettingsViewModel viewModel;
        public ProfileSettingsView(ProfileSettingsViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }

        public async Task Initialize(GameProfile p)
        {
            await viewModel.Initialize(p);
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
