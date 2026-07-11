using System.Windows;
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
            await viewModel.ShowNewProfileWindow();
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
            }
            else
            {
                WindowState = WindowState.Maximized;
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.CheckForUpdatesAsync();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Update();
        }
    }
}