using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Model.DB;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class AddServerView : UserControl
    {
        public AddServerViewModel viewModel => DataContext as AddServerViewModel;
        public AddServerView()
        {
            InitializeComponent();
        }

        private CancellationTokenSource cT;

        public Server? CreatedServer { get; set; }

        private async void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            string sName = ServerNameInput.Text;
            if (string.IsNullOrEmpty(sName))
            {
                MessageBox.Show("Give the server a name");
                return;
            }
            string ip = ServerIPInput.Text;
            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Set IP/Domain for the server");
                return;
            }
            else if (!NetworkUtil.IsValidServerAddress(ip))
            {
                MessageBox.Show("Set valid IP/Domain for the server");
                return;
            }
            string version = MCVersionCombo.SelectedItem?.ToString();
            if(string.IsNullOrEmpty(version))
            {
                MessageBox.Show("Set a minecraft version");
                return;
            }
            GameProfile profile = ProfileCombo.SelectedItem as GameProfile;

            await viewModel.AddServer();
        }

        public async Task InitializeDataAsync()
        {
            await viewModel.InitializeDataAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ClosePanel();
        }

        private async void ServerIPInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            cT?.Cancel();

            cT = new CancellationTokenSource();
            var token = cT.Token;

            string address = ServerIPInput.Text;

            try
            {
                await Task.Delay(500, token);
                if (viewModel == null) return;
                await viewModel.OnServerIPChanged();
            }
            catch (TaskCanceledException)
            {
                
            }
        }

        private async void MCVersionCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (viewModel != null) await viewModel.LoadProfiles();
        }

        private void ResetProfile_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ResetBindedProfile();
        }
    }
}
