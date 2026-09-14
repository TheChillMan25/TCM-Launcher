using System.Collections.ObjectModel;
using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.Mods;
using TCM_Launcher.MVVM.ViewModel;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.Interfaces
{
    public interface IOverlayService
    {
        public event Action<ViewModelBase>? ShowOverlayRequested;
        public event Action? CloseOverlayRequested;

        Task<string?> ShowExportModpackPanelAsync(GameProfile profile, List<ProfileModViewModel> allProfileMods);
        void ShowFriendSearch();
        Task<(string? filePath, ProfileModInfo? modInfo)?> ShowModImportPanel(ProfileModInfo? details = null);
        Task ShowModpacksPanelAsync();
        Task<ProfileModInfo?> ShowModSettingsPanel(ProfileModInfo mod);
        Task<GameProfile?> ShowNewProfilePanelAsync();
        void ShowNotificaionsPanel(ObservableCollection<FirebaseNotification> notifications);
        Task ShowProfileSettingsAsync(GameProfile profile);
        Task<bool?> ShowPopupPanelAsync(string title, string text, PopupAction action, double? time = null, string? profileId = null);
        Task<Server?> ShowServerPanelAsync(Server? serverToEdit = null);
        void ShowLauncherLoadingPanel(LoadingScreenViewModel vm);
    }
}
