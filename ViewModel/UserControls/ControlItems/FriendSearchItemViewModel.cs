using TCM_Launcher.Interfaces;
using TCM_Launcher.MVVM.ViewModel;

namespace TCM_Launcher.ViewModel.UserControls.ControlItems
{
    public class FriendSearchItemViewModel : ViewModelBase
    {
		private readonly IBackendService backendService;

        public FriendSearchItemViewModel(IBackendService backendService)
        {
            this.backendService = backendService;
        }

        public Action<string> SentFriendRequest { get; set; }

        private string uuid;
		public string UUID
		{
			get { return uuid; }
			set { uuid = value; OnPropertyChange(); }
		}

		private string username;

		public string Username
		{
			get { return username; }
			set { username = value; OnPropertyChange(); }
		}

		public async Task AddFriendAsync()
		{
            if (await backendService.AddFriendAsync(UUID) != null)
			{
				SentFriendRequest.Invoke(UUID);
			}
		}
	}
}
