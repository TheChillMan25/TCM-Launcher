using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel;

namespace TCM_Launcher
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel;
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
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
                BottomSpacer.Height = 0;
            }
            else
            {
                WindowState = WindowState.Maximized;
                BottomSpacer.Height = 55;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.CheckForUpdatesAsync();
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.Update();
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