using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TCM_Launcher.Model;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.ViewModel.UserControls.Panels
{
    public class NotificationsViewModel : ViewModelBase
    {
        public Action OnPanelCloseRequested { get; set; }

        private ObservableCollection<NotificationViewModel> notifications;

		public ObservableCollection<NotificationViewModel> Notifications
		{
			get { return notifications; }
			set { notifications = value; OnPropertyChange(); }
		}

		public void Initialize(ObservableCollection<FirebaseNotification> notifications)
		{
			var list = notifications.Select(notification =>
			{
				var vm = App.ServiceProvider.GetRequiredService<NotificationViewModel>();
				vm.OnRemoveRequestRequested = RemoveNotification;
				vm.Initialize(notification);
				return vm;
			}).ToList();
			Notifications = new(list);
		}

        private void RemoveNotification(string requestId)
        {
			var existring = Notifications.FirstOrDefault(n => n.RequestId == requestId);
			if (existring != null) Notifications.Remove(existring);
			if (Notifications.Count == 0) OnPanelCloseRequested?.Invoke();
        }

        public void Close()
        {
			OnPanelCloseRequested?.Invoke();
        }
    }
}
