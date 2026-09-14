using TCM_Launcher.Core.Utils.Converters;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model;
using TCM_Launcher.MVVM.ViewModel;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class NotificationViewModel : ViewModelBase
    {
        private readonly IBackendService backendService;
        public NotificationViewModel(IBackendService backendService)
        {
            this.backendService = backendService;
        }

        public string RequestId { get; set; }
        public string Sender { get; set; }
        public string NotificationText { get; set; }
        public string Date { get; set; }
        public Action<string> OnRemoveRequestRequested { get; set; }

        public void Initialize(FirebaseNotification notification)
        {
            RequestId = notification.Id;
            Sender = notification.SenderName;
            NotificationText = FirebaseNotification.ConvertEnumToString(notification.Type);
            Date = TimestampConverter.ConvertToString(notification.CreatedAt);
        }

        public async Task Accept()
        {
            await backendService.UpdateFriendRequestAsync(RequestId, true);
            OnRemoveRequestRequested?.Invoke(RequestId);
        }

        public async Task Reject()
        {
            await backendService.UpdateFriendRequestAsync(RequestId, false);
            OnRemoveRequestRequested?.Invoke(RequestId);
        }
    }
}
