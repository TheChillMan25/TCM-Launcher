using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.ViewModel;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.Services
{
    public class OverlayService : IOverlayService
    {
        private readonly IServiceProvider serviceProvider;

        public OverlayService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public event Action<ViewModelBase>? ShowOverlayRequested;
        public event Action? CloseOverlayRequested;

        public async Task<string?> ShowExportModpackPanelAsync(GameProfile profile, List<ProfileModViewModel> allProfileMods)
        {
            if (App.Current.Dispatcher == null) return null;
            var tcs = new TaskCompletionSource<string?>();
            await App.Current.Dispatcher.InvokeAsync(async () =>
            {
                var vm = serviceProvider.GetRequiredService<ExportModpackViewModel>();
                await vm.Initialize(profile, allProfileMods);

                vm.OnPanelCloseRequested = modpackId =>
                {
                    CloseOverlayRequested?.Invoke();
                    tcs.SetResult(modpackId);
                };
                ShowOverlayRequested?.Invoke(vm);
            });
            return await tcs.Task;
        }

        public void ShowFriendSearch()
        {
            App.Current.Dispatcher?.Invoke(() =>
            {
                var vm = serviceProvider.GetRequiredService<SearchFriendsViewModel>();

                vm.OnPanelCloseRequested = () => CloseOverlayRequested?.Invoke();
                ShowOverlayRequested?.Invoke(vm);
            });
        }

        public Task<(string? filePath, ProfileModInfo? modInfo)?> ShowModImportPanel(ProfileModInfo? details = null)
        {
            var tcs = new TaskCompletionSource<(string? filePath, ProfileModInfo? modInfo)?>();

            App.Current.Dispatcher?.Invoke(() =>
            {
                var vm = serviceProvider.GetRequiredService<ModDetailsViewModel>();
                vm.Initialize(details);

                vm.OnPanelCloseRequested = (info) =>
                {
                    CloseOverlayRequested?.Invoke();
                    tcs.SetResult(info != null ? (info.Value.filePath, info.Value.modInfo) : null);
                };
                ShowOverlayRequested?.Invoke(vm);
            });
            return tcs.Task;
        }

        public async Task ShowModpacksPanelAsync()
        {
            if (App.Current?.Dispatcher == null) return;
            await App.Current.Dispatcher.InvokeAsync(async () =>
            {
                var vm = serviceProvider?.GetRequiredService<ModpacksViewModel>();
                await vm.Initialize();
                vm.OnPanelCloseRequested = () => CloseOverlayRequested?.Invoke();
                ShowOverlayRequested?.Invoke(vm);
            });
        }

        public Task<ProfileModInfo?> ShowModSettingsPanel(ProfileModInfo mod)
        {
            var tcs = new TaskCompletionSource<ProfileModInfo?>();

            App.Current?.Dispatcher?.Invoke(() =>
            {
                var vm = serviceProvider.GetRequiredService<ModDetailsViewModel>();
                vm.Initialize(mod, false);

                vm.OnPanelCloseRequested = (info) =>
                {
                    if(info != null)
                    {
                        CloseOverlayRequested?.Invoke();
                        tcs.SetResult(info.Value.modInfo);
                    }
                };
                ShowOverlayRequested?.Invoke(vm);
            });
            return tcs.Task;
        }

        public async Task<GameProfile?> ShowNewProfilePanelAsync()
        {
            if (App.Current?.Dispatcher == null) return null;

            var tcs = new TaskCompletionSource<GameProfile?>();

            await App.Current.Dispatcher.InvokeAsync(async () =>
            {
                var vm = serviceProvider.GetRequiredService<NewProfileViewModel>();
                await vm.InitializeVersions();
                vm.OnPanelCloseRequested = (profile) =>
                {
                    CloseOverlayRequested?.Invoke();
                    tcs.SetResult(profile);
                };
                ShowOverlayRequested?.Invoke(vm);
            });
            return await tcs.Task;
        }

        public void ShowNotificaionsPanel(ObservableCollection<FirebaseNotification> notifications)
        {
            App.Current.Dispatcher?.Invoke(() =>
            {
                var vm = serviceProvider.GetRequiredService<NotificationsViewModel>();
                vm.Initialize(notifications);
                vm.OnPanelCloseRequested = () => CloseOverlayRequested?.Invoke();
                ShowOverlayRequested?.Invoke(vm);
            });
        }

        public async Task ShowProfileSettingsAsync(GameProfile profile)
        {
            if (App.Current?.Dispatcher == null) return;

            await App.Current.Dispatcher.InvokeAsync(async () => 
            {
                var vm = serviceProvider.GetRequiredService<ProfileSettingsViewModel>();
                await vm.Initialize(profile);
                vm.OnPanelCloseRequested = () => CloseOverlayRequested?.Invoke();
                ShowOverlayRequested?.Invoke(vm);
            });
        }

        public Task<bool?> ShowPopupPanelAsync(string title, string text, PopupAction action, double? time = null, string? profileId = null)
        {
            try
            {
                var tcs = new TaskCompletionSource<bool?>();

                App.Current.Dispatcher.Invoke(() =>
                {
                    var vm = serviceProvider.GetRequiredService<PopupViewModel>();
                    vm.Initialize(title, text, action, time, profileId);

                    vm.OnPanelCloseRequested = (result) =>
                    {
                        CloseOverlayRequested?.Invoke();
                        tcs.SetResult(result);
                    };

                    ShowOverlayRequested?.Invoke(vm);
                });
                return tcs.Task;
            }
            catch (Exception ex)
            {
                Constants.MessageBoxError(ex.Message);
                return null;
            }
        }

        public async Task<Server?> ShowServerPanelAsync(Server? serverToEdit = null)
        {
            if (App.Current?.Dispatcher == null) return null;

            var tcs = new TaskCompletionSource<Server?>();

            await App.Current.Dispatcher.InvokeAsync(async () =>
            {
                var vm = serviceProvider.GetRequiredService<AddServerViewModel>();
                await vm.InitializeDataAsync();

                if(serverToEdit != null)
                {
                    vm.Initialize(serverToEdit);
                }

                vm.OnPanelCloseRequested = (server) =>
                {
                    CloseOverlayRequested?.Invoke();
                    tcs.SetResult(server);
                };
                ShowOverlayRequested?.Invoke(vm);
            });
            return await tcs.Task;
        }

        public void ShowLauncherLoadingPanel(LoadingScreenViewModel vm)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                vm.OnPanelCloseRequested = () => CloseOverlayRequested?.Invoke();
                ShowOverlayRequested?.Invoke(vm);
            });
        }
    }
}
