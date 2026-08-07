using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Forms = System.Windows.Forms;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Services;
using TCM_Launcher.View;
using TCM_Launcher.View.PopUp;
using TCM_Launcher.View.UserControls;
using TCM_Launcher.View.UserControls.ControlItems;
using TCM_Launcher.View.UserControls.Sidebars;
using TCM_Launcher.View.Windows;
using TCM_Launcher.ViewModel;
using TCM_Launcher.ViewModel.Popup;
using TCM_Launcher.ViewModel.UserControls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCM_Launcher.ViewModel.UserControls.Sidebars;
using TCM_Launcher.ViewModel.Windows;

namespace TCM_Launcher
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        private Forms.NotifyIcon notifyIcon;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            notifyIcon = new Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon("Assets\\TCM.ico"),
                Text = "TCM Launcher",
                Visible = true,
            };

            notifyIcon.DoubleClick += (s, args) =>
            {
                ShowMainWindow();
            };

            var contextMenu = new Forms.ContextMenuStrip();
            contextMenu.Items.Add("Open", null, (s, args) => ShowMainWindow());
            contextMenu.Items.Add("Exit", null, (s, args) => Shutdown());
            notifyIcon.ContextMenuStrip = contextMenu;

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            using var db = new LauncherDBContext();

            db.Database.Migrate();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        public void ShowMainWindow()
        {
            if (MainWindow != null)
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Maximized;
                MainWindow.Activate();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose();
            base.OnExit(e);
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Services //
            services.AddSingleton<IGameProfileService, GameProfileService>();
            services.AddSingleton<IProfileSettingsService, ProfileSettingsService>();
            services.AddSingleton<IServerService, ServerService>();
            services.AddSingleton<IVersionService, VersionsService>();
            services.AddSingleton<IMicrosoftService, MicrosoftService>();
            services.AddSingleton<ILauncherService, LauncherService>();
            services.AddSingleton<IBackendService, BackendService>();
            services.AddSingleton<IDownloadService, DownloadService>();
            services.AddSingleton<IProfileModService, ProfileModService>();
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddSingleton<IAppMetaDataService, AppMetaDataService>();

            // ViewModels //
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<NewProfileViewModel>();
            services.AddTransient<ProfileSettingsViewModel>();
            services.AddTransient<NewProfileViewModel>();
            services.AddTransient<AddServerViewModel>();
            services.AddTransient<AddContentViewModel>();
            services.AddTransient<ModSearchResultCardViewModel>();
            services.AddTransient<ModDetailsViewModel>();
            services.AddTransient<ModVersionCardViewModel>();
            services.AddTransient<ProfileModViewModel>();
            services.AddTransient<ProfilesViewModel>();
            services.AddTransient<LeftSidebarViewModel>();
            services.AddTransient<RightSidebarViewModel>();
            services.AddTransient<PinnedProfileViewModel>();
            services.AddTransient<PopupViewModel>();
            services.AddTransient<AppSettingsViewModel>();
            services.AddTransient<ImportModViewModel>();
            services.AddTransient<ProfileInstallIndicatorViewModel>();

            // UserControls //
            services.AddTransient<ServerCardViewModel>();
            services.AddTransient<ProfileDetailsViewModel>();
            services.AddTransient<ModSearchResultCardView>();
            services.AddTransient<ModDetailsView>();
            services.AddTransient<ModVersionCardView>();
            services.AddTransient<ProfileModView>();
            services.AddTransient<ProfilesViewModel>();
            services.AddTransient<LeftSidebarView>();
            services.AddTransient<RightSidebarView>();
            services.AddTransient<PinnedProfileView>();
            services.AddTransient<PopupView>();
            services.AddTransient<AppSettingsViewModel>();
            services.AddTransient<ProfileInstallIndicatiorView>();

            // Windows //
            services.AddTransient<MainWindow>();
            services.AddTransient<NewProfileView>();
            services.AddTransient<ProfileSettingsView>();
            services.AddTransient<NewProfileView>();
            services.AddTransient<AddServerView>();
            services.AddTransient<AddContentView>();
            services.AddTransient<ImportModView>();

        }
    }

}
