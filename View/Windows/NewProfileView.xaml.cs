using System.Windows;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.DB.Versions;
using TCM_Launcher.ViewModel.UI.Windows;

namespace TCM_Launcher.View
{
    public partial class NewProfileView : Window
    {
        private NewProfileViewModel viewModel;

        public GameProfile NewProfileData { get; set; }

        public NewProfileView()
        {
            InitializeComponent();
            viewModel = new NewProfileViewModel();
            DataContext = viewModel;
        }


        private async void VersionCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (VersionCombo.SelectedItem == null)
            {
                return;
            }
            
            if(ForgeVersionCombo != null) {
                if(VersionCombo.SelectedItem is VanillaVersion selectedVanilla)
                {
                    string pName = ProfileNameInput.Text;
                    viewModel.UpdateForgeVersions(selectedVanilla.VersionName, pName);
                    ForgeVersionCombo.SelectedIndex = 0;
                }
            }
        }

        private async void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if(VersionCombo.SelectedItem is VanillaVersion selectedVanilla && ForgeVersionCombo.SelectedItem is ForgeVersion selectedForge)
            {
                string pName = ProfileNameInput.Text;
                if (selectedForge == null)
                {
                    MessageBox.Show("There is no compatible forge for this version. Select another version.");
                    return;
                }
                NewProfileData = new GameProfile
                {
                    ProfileName = string.IsNullOrEmpty(pName) ? string.Concat("Forge ", selectedVanilla.VersionName) : pName,
                    MCVersion = selectedVanilla.VersionName,
                    ForgeVersion = selectedForge.VersionName,
                };
                CreateProfileButton.IsEnabled = false;
                MessageBox.Show("Installation started");
                DialogResult = true;
                Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
