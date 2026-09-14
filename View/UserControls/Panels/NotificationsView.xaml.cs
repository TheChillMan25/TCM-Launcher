using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.Model;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class NotificationsView : UserControl
    {
        private NotificationsViewModel viewModel => DataContext as NotificationsViewModel;
        public NotificationsView()
        {
            InitializeComponent();
        }

        public void Initialize(ObservableCollection<FirebaseNotification> notifications)
        {
            viewModel.Initialize(notifications);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }
    }
}
