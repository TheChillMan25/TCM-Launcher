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
                System.Windows.MessageBox.Show(
                    $"Critical error. Check logs for details.",
                    "Launcher Crash",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Logger.Error("There was a critical error at launch.", e.Exception);
                e.Handled = true;
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

            var profile = await profileModService.ImportModpackDirectlyAsync(filePath);
            if (profile != null)
            {
                var profilesView = ServiceProvider.GetRequiredService<ProfilesViewModel>();
                var installVm = ServiceProvider.GetRequiredService<ProfileInstallIndicatorViewModel>();

                var progress = new Progress<double>(progress => installVm.Progress = progress);
                var status = new Progress<string>(status => installVm.Status = status);
                var visible = new Progress<bool>(visible => installVm.Visible = visible);

                installVm.ProfileName = profile.ProfileName;
                installVm.ProfileId = profile.Id;
                installVm.OnRemoveRequested = () =>
                {
                    var existing = profilesView.Installs.FirstOrDefault(vm => vm.ProfileId == installVm.ProfileId);
                    if (existing != null) profilesView.Installs.Remove(existing);
                };
                profilesView.Installs.Add(installVm);

                var launcherService = ServiceProvider.GetRequiredService<ILauncherService>();

                var fileName = await launcherService.CreateProfileAsync(profile.Id, profile.MCVersion, profile.ForgeVersion, progress, status, visible);
                if (fileName != null)
                {
                    var gameProfileService = ServiceProvider.GetRequiredService<IGameProfileService>();
                    profile.Installed = true;
                    profile.FileName = fileName;
                    await gameProfileService.UpdateProfileAsync(profile.Id, profile);
                    var vm = App.ServiceProvider.GetRequiredService<ProfileDetailsViewModel>();
                    vm.Profile = profile;
                    profilesView.Profiles.Add(vm);
                }
            }
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
            services.AddSingleton<IAppMetaDataService, AppMetaDataService>();

            // ViewModels //
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<NewProfileViewModel>();
            services.AddTransient<ProfileSettingsViewModel>();
            services.AddTransient<NewProfileViewModel>();
            services.AddTransient<AddServerViewModel>();
            services.AddTransient<AddContentViewModel>();
            services.AddTransient<ModSearchResultCardViewModel>();
            services.AddTransient<ViewModel.UserControls.SearchedModDetailsViewModel>();
            services.AddTransient<ModVersionCardViewModel>();
            services.AddTransient<ProfileModViewModel>();
            services.AddSingleton<ProfilesViewModel>();
            services.AddTransient<LeftSidebarViewModel>();
            services.AddTransient<RightSidebarViewModel>();
            services.AddTransient<PinnedProfileViewModel>();
            services.AddTransient<PopupViewModel>();
            services.AddTransient<AppSettingsViewModel>();
            services.AddTransient<ViewModel.Windows.ModDetailsViewModel>();
            services.AddTransient<ProfileInstallIndicatorViewModel>();
            services.AddTransient<PopupViewModel>();
            services.AddTransient<ExportModpackViewModel>();

            // UserControls //
            services.AddTransient<ServerCardViewModel>();
            services.AddTransient<ProfileDetailsViewModel>();
            services.AddTransient<ModSearchResultCardView>();
            services.AddTransient<View.UserControls.SearchedModDetailsView>();
            services.AddTransient<ModVersionCardView>();
            services.AddTransient<ProfileModView>();
            services.AddTransient<ProfilesView>();
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
            services.AddTransient<View.Windows.ModDetailsView>();
            services.AddTransient<PopupView>();
            services.AddTransient<ExportModpackView>();

        }
    }

}
