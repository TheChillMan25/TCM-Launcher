using CmlLib.Core.Auth;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.Services;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCM_Launcher.ViewModel.UserControls.Panels;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel.UserControls.Sidebars
{
    public class RightSidebarViewModel : ViewModelBase
    {
        private readonly IMicrosoftService microsoftService;
        private readonly IFirebaseService firebaseService;
        private readonly IBackendService backendService;
        private readonly IOverlayService overlayService;
        public RightSidebarViewModel(IMicrosoftService microsoftService, IFirebaseService firebaseService, IBackendService backendService, IOverlayService overlayService)
        {
            this.microsoftService = microsoftService;
            this.firebaseService = firebaseService;
            this.backendService = backendService;
            this.overlayService = overlayService;
        }

        private string username;
        public string Username
        {
            get { return username; }
            set { username = value; OnPropertyChange(); }
        }

        private string offlineUsername = "User123";
        public bool IsLoggedIn => MSession != null;

        private MSession mSession;
        public MSession MSession
        {
            get { return mSession; }
            set 
            {
                mSession = value;
                OnPropertyChange();
                OnPropertyChange(nameof(IsLoggedIn));
                UpdateIcon();
            }
        }

        private ObservableCollection<FriendViewModel> friends;
        public ObservableCollection<FriendViewModel> Friends
        {
            get { return friends; }
            set { friends = value; OnPropertyChange(); }
        }

        private ObservableCollection<FirebaseNotification> notifications = new();
        public ObservableCollection<FirebaseNotification> Notifications
        {
            get { return notifications; }
            set { notifications = value; }
        }

        public bool HasNotifications => Notifications.Count > 0;

        private Geometry buttonIcon;
        public Geometry ButtonIcon
        {
            get { return buttonIcon; }
            set { buttonIcon = value; OnPropertyChange(); }
        }

        private ObservableCollection<FirestoreModpack> modpacks = new();

        public ObservableCollection<FirestoreModpack> Modpacks
        {
            get { return modpacks; }
            set { modpacks = value; OnPropertyChange(); }
        }

        private void UpdateIcon()
        {
            string key = IsLoggedIn ? "LogoutIcon" : "MicrosoftIcon";

            if (App.Current != null)
            {
                ButtonIcon = App.Current.FindResource(key) as Geometry;
            }
        }

        public async Task MicrosoftLoginAsync(bool silent = false)
        {
            MSession = await microsoftService.MicrosoftLoginAsync(silent);
            if (MSession != null && !string.IsNullOrEmpty(MSession.Username))
            {
                Username = MSession.Username;
            }else Username = offlineUsername;
        }
        public async Task MicrosoftLogoutAsync()
        {
            bool success = await microsoftService.MicrosoftSignOutAsync();
            if (success)
            {
                MSession = null;
                Username = offlineUsername;
                OnPropertyChange(nameof(IsLoggedIn));
            }
        }
        public async Task OnUserSessionButtonClick()
        {
            try
            {
                if (IsLoggedIn)
                {
                    await MicrosoftLogoutAsync();
                }
                else
                {
                    await MicrosoftLoginAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception.", ex);
            }
        }

        public async Task InitializeListener()
        {
            await firebaseService.ListenToRequestsAsync(
                microsoftService.MSession.UUID,
                onAdded: request =>
                {
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        Notifications.Add(new FirebaseNotification
                        {
                            Id = request.Id,
                            Sender = request.Sender,
                            SenderName = request.SenderName,
                            CreatedAt = request.CreatedAt,
                            Type = request.RequestType,
                        });
                        OnPropertyChange(nameof(HasNotifications));
                    });
                },
                onRemoved: request =>
                {
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        var existing = Notifications.FirstOrDefault(n => n.Id == request);
                        if (existing != null)
                        {
                            Notifications.Remove(existing);
                            OnPropertyChange(nameof(HasNotifications));
                        }
                    });
                }
            );
            await firebaseService.ListenToFriendsList(
                microsoftService.MSession.UUID,
                onChange: change =>
                {
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        var friendsList = change.Select(f =>
                        {
                            var vm = App.ServiceProvider.GetRequiredService<FriendViewModel>();
                            vm.UUID = f.UUID;
                            vm.Username = f.Username;
                            vm.OnRemoveFriendRequested = RemoveFriendAsync;
                            return vm;
                        }).ToList();
                        Friends = new(friendsList);
                    });
                });
            await firebaseService.ListenToModpacksAsync(
                microsoftService.MSession.UUID,
                onChange: change =>
                {
                    App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Modpacks = new(change);
                    });
                });
        }

        private async Task RemoveFriendAsync(string uuid)
        {
            await backendService.RemoveFriendAsync(uuid);
        }

        public void ShowFriendSearch()
        {
            overlayService.ShowFriendSearch();
        }

        public void ShowNotifications()
        {
            overlayService.ShowNotificaionsPanel(Notifications);
        }

        public async Task ShowModpacksAsync()
        {
            await overlayService.ShowModpacksPanelAsync();
        }
    }
}
