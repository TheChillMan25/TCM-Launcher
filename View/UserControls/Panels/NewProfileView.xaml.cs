using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.DB.Versions;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class NewProfileView : UserControl
    {
        private NewProfileViewModel viewModel => DataContext as NewProfileViewModel;

        public NewProfileView()
        {
            InitializeComponent();
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
                    await viewModel.UpdateForgeVersions(selectedVanilla.VersionName, pName);
                    ForgeVersionCombo.SelectedIndex = 0;
                }
            }
        }

        private async void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CreateProfile();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }
    }
}
