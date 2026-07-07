using System.Windows;
using TCM_Launcher.Model.DB;
using TCM_Launcher.ViewModel.UI;

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string pName = ProfileNameInput.Text;
            int ram = (int)RamSlider.Value;
            string jvmArgs = JVMArgInput.Text;
            viewModel.SaveSettings(pName, ram, jvmArgs);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
