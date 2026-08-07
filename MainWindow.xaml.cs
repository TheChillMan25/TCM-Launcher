using System.Windows;
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

        private void HeaderBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else DragMove();
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
            viewModel.CloseButtonClick();
        }
        
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await viewModel.OnLoadedAsync();
        }
    }
}