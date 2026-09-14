using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.View.UserControls.ControlItems
{
    public partial class NotificationView : UserControl
    {
        private NotificationViewModel viewModel => DataContext as NotificationViewModel;
        public NotificationView()
        {
            InitializeComponent();
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.Accept();
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.Reject();
        }
    }
}
