using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.View.UserControls;
using TCM_Launcher.ViewModel;

namespace TCM_Launcher
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel;
        public MainWindow()
        {
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            InitializeComponent();
        }

        private async void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.OpenNewProfileWindow();
        }

        private void HeaderBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if(WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                BugReportButton.Margin = new Thickness(0, 0, 0, 5);
            }
            else
            {
                WindowState = WindowState.Maximized;
                BugReportButton.Margin = new Thickness(0, 0 , 0, 60);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DeleteProfileEventHandler(object sender, RoutedEventArgs e)
        {
            viewModel.DeleteProfile();
        }

        private void DeleteServerEventHandler(object sender, RoutedEventArgs e)
        {
            if(sender is ServerCardView card)
            {
                viewModel.DeleteServer(card.viewModel.Server);
            }
        }

        private void UpdatedServerEventHandler(object sender, RoutedEventArgs e)
        {
            if (sender is ServerCardView card)
            {
                viewModel.UpdateServer(card.viewModel.Server);
            }
        }

        private void QuickLaunchEventHandler(object sender, RoutedEventArgs e)
        {
            if (sender is ServerCardView card)
            {
                viewModel.UpdateProfile(card.viewModel.Server.BindedProfileId);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.CheckForUpdatesAsync();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Update();
        }

        private void BugReportButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Bugreport();
        }

        private async void MicrosoftLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.MicrosoftLoginAsync();
        }

        private void MicrosoftAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.VerticalOffset = 5;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }

        }

        private async void MicrosoftLogoutButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.MicrosoftLogoutAsync();
        }

        public async void CheckNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.CheckNetworkAsync();
        }

        private async void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.OpenAddServerWindowAsync();
        }
    }
}