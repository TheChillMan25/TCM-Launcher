using System.Windows;
using TCM_Launcher.Services;
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

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedGameProfile == null) 
            { 
                MessageBox.Show("Select a profile to START.");
                return;
            }
            if (viewModel.SelectedGameProfile.Installed != true)
            {
                MessageBox.Show("Profile is installing files. Please wait");
                return;
            }

            await LauncherService.Instance.LaunchProfileAsync(viewModel.SelectedGameProfile.Id, viewModel.SelectedGameProfile.FileName!);
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OpenProfileSettings();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.DeleteProfile();
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
    }
}