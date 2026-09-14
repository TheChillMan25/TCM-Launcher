using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Windows;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Services;
using TCM_Launcher.View.UserControls;
using TCM_Launcher.View.UserControls.ControlItems;
using TCM_Launcher.View.UserControls.Panels;
using TCM_Launcher.View.UserControls.Sidebars;
using TCM_Launcher.ViewModel;
using TCM_Launcher.ViewModel.UserControls;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCM_Launcher.ViewModel.UserControls.Panels;
using TCM_Launcher.ViewModel.UserControls.Sidebars;
using Forms = System.Windows.Forms;

namespace TCM_Launcher
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        private Forms.NotifyIcon notifyIcon;

        private const string MutexName = "TCM_Launcher_SingleInstance_Mutex";
        private const string PipeName = "TCM_Launcher_SingleInstance_Pipe";
        private static Mutex? mutex;
        private CancellationTokenSource? pipeCts;

        public App()
        {
            this.DispatcherUnhandledException += (sender, e) =>
            {
                Logger.Error("There was a critical error at launch.", e.Exception);
                System.Windows.MessageBox.Show(
                    $"Critical error. Check logs for details.",
                    "Launcher Crash",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Logger.Error("Unobserved task exception in background task.", e.Exception);
                e.SetObserved();
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            mutex = new Mutex(true, MutexName, out bool isFirstInstance);

            if (!isFirstInstance)
            {
                if (e.Args.Length > 0 && File.Exists(e.Args[0]))
                {
                    SendFilePathToRunningInstance(e.Args[0]);
                }
                Shutdown();
                return;
            }

            pipeCts = new CancellationTokenSource();
            StartNamedPipeServer(pipeCts.Token);

            base.OnStartup(e);

            var iconUri = new Uri("pack://application:,,,/Assets/TCM.ico", UriKind.Absolute);
            var streamInfo = System.Windows.Application.GetResourceStream(iconUri);
            notifyIcon = new Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(streamInfo.Stream),
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

            using (var db = new LauncherDBContext())
            {
                db.Database.Migrate();
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            if (e.Args.Length > 0 && File.Exists(e.Args[0]) && Path.GetExtension(e.Args[0]).Equals(".tcmp", StringComparison.OrdinalIgnoreCase))
            {
                await HandleModpackImportAsync(e.Args[0]);
            }
        }

        private async Task HandleModpackImportAsync(string filePath)
        {
            var profileModService = ServiceProvider.GetRequiredService<IProfileModService>();

            await profileModService.ImportModpackDirectlyAsync(filePath);
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

            pipeCts?.Cancel();
            mutex?.ReleaseMutex();
            mutex?.Dispose();
            notifyIcon?.Dispose();
            var firebaseService = ServiceProvider.GetRequiredService<IFirebaseService>();
            firebaseService?.StopListeningAsync();
            base.OnExit(e);
        }

        private static void SendFilePathToRunningInstance(string filePath)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(1000);
                using var writer = new StreamWriter(client, Encoding.UTF8);
                writer.WriteLine(filePath);
                writer.Flush();
            }
            catch { }
        }

        private void StartNamedPipeServer(CancellationToken token)
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                        await server.WaitForConnectionAsync(token);

                        using var reader = new StreamReader(server, Encoding.UTF8);
                        string? filePath = await reader.ReadLineAsync();

                        if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                        {
                            await Dispatcher.InvokeAsync(async () =>
                            {
                                ShowMainWindow();
                                await HandleModpackImportAsync(filePath);
                            });
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch { }
                }
            }, token);
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
            services.AddSingleton<IDownloadedModpacksService, DownloadedModpacksService>();
            services.AddSingleton<IFirebaseService, FirebaseService>();
            services.AddSingleton<IOverlayService, OverlayService>();

            // ViewModels //
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<NewProfileViewModel>();
            services.AddTransient<ProfileSettingsViewModel>();
            services.AddTransient<NewProfileViewModel>();
            services.AddTransient<AddServerViewModel>();
            services.AddTransient<AddContentViewModel>();
            services.AddTransient<ModSearchResultCardViewModel>();
            services.AddTransient<SearchedModDetailsViewModel>();
            services.AddTransient<ModVersionCardViewModel>();
            services.AddTransient<ProfileModViewModel>();
            services.AddSingleton<ProfilesViewModel>();
            services.AddTransient<LeftSidebarViewModel>();
            services.AddTransient<RightSidebarViewModel>();
            services.AddTransient<PinnedProfileViewModel>();
            services.AddTransient<PopupViewModel>();
            services.AddTransient<AppSettingsViewModel>();
            services.AddTransient<ModDetailsViewModel>();
            services.AddTransient<ProfileInstallIndicatorViewModel>();
            services.AddTransient<PopupViewModel>();
            services.AddTransient<ExportModpackViewModel>();
            services.AddTransient<SearchFriendsViewModel>();
            services.AddTransient<FriendSearchItemViewModel>();
            services.AddTransient<NotificationsViewModel>();
            services.AddTransient<FriendViewModel>();
            services.AddSingleton<ModpacksViewModel>();
            services.AddTransient<ModpackViewModel>();
            services.AddTransient<LoadingScreenViewModel>();

            // UserControls //
            services.AddTransient<ServerCardViewModel>();
            services.AddTransient<ProfileDetailsViewModel>();
            services.AddTransient<ModSearchResultCardView>();
            services.AddTransient<SearchedModDetailsView>();
            services.AddTransient<ModVersionCardView>();
            services.AddTransient<ProfileModView>();
            services.AddTransient<ProfilesView>();
            services.AddTransient<LeftSidebarView>();
            services.AddTransient<RightSidebarView>();
            services.AddTransient<PinnedProfileView>();
            services.AddTransient<AppSettingsViewModel>();
            services.AddTransient<ProfileInstallIndicatiorView>();
            services.AddTransient<FriendSearchItemView>();
            services.AddTransient<NotificationViewModel>();
            services.AddTransient<FriendView>();
            services.AddTransient<NewProfileView>();
            services.AddTransient<ProfileSettingsView>();
            services.AddTransient<NewProfileView>();
            services.AddTransient<AddServerView>();
            services.AddTransient<AddContentView>();
            services.AddTransient<ModDetailsView>();
            services.AddTransient<PopupView>();
            services.AddTransient<ExportModpackView>();
            services.AddTransient<AddFriendView>();
            services.AddTransient<NotificationsView>();
            services.AddSingleton<ModpacksView>();
            services.AddTransient<ModpackView>();
            services.AddTransient<LoadingScreenView>();

            // Windows //
            services.AddTransient<MainWindow>();

        }
    }

}
