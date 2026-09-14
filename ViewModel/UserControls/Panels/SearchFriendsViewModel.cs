using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.ViewModel.UserControls.Panels
{
    public class SearchFriendsViewModel : ViewModelBase
    {
        private readonly IBackendService backendService;
        public SearchFriendsViewModel(IBackendService backendService)
        {
            this.backendService = backendService;
        }

        public Action OnPanelCloseRequested { get; set; }

        private CancellationTokenSource cts;

        private string friendSearchText;
        public string FriendSearchText
        {
            get { return friendSearchText; }
            set { friendSearchText = value; OnPropertyChange(); _ = SearchAsync(value); }
        }

        private ObservableCollection<FriendSearchItemViewModel> friendSearchResult = new();

        public ObservableCollection<FriendSearchItemViewModel> FriendSearchResult
        {
            get { return friendSearchResult; }
            set { friendSearchResult = value; OnPropertyChange(); }
        }

        public async Task SearchAsync(string username)
        {
            cts?.Cancel();

            cts = new CancellationTokenSource();
            var token = cts.Token;

            try
            {
                await Task.Delay(500, token);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrWhiteSpace(username))
                {
                    var list = await backendService.SearchFriendsAsync(username.Trim());
                    if(list != null)
                    {
                        List<FriendSearchItemViewModel> vmList = list.Select(user =>
                        {
                            FriendSearchItemViewModel vm = App.ServiceProvider.GetRequiredService<FriendSearchItemViewModel>();
                            vm.UUID = user.UUID;
                            vm.Username = user.Username;
                            vm.SentFriendRequest = RemoveUser;
                            return vm;
                        }).ToList();
                        FriendSearchResult = new(vmList);
                    }
                }
                else
                {
                    FriendSearchResult.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when searching for friends.", ex);
            }
        }

        private void RemoveUser(string uuid)
        {
            FriendSearchItemViewModel? existing = FriendSearchResult.FirstOrDefault(u => u.UUID == uuid);
            if (existing != null)
            {
                FriendSearchResult.Remove(existing);
            }
        }
    }
}
