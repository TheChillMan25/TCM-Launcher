using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Services;
using TCM_Launcher.View;
using TCM_Launcher.View.UserControls;
using TCM_Launcher.View.Windows;
using TCM_Launcher.ViewModel;
using TCM_Launcher.ViewModel.UserControls;
using TCM_Launcher.ViewModel.Windows;

namespace TCM_Launcher
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            using var db = new LauncherDBContext();

            db.Database.Migrate();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<IGameProfileService, GameProfileService>();
            services.AddSingleton<IProfileSettingsService, ProfileSettingsService>();
            services.AddSingleton<IServerService, ServerService>();
            services.AddSingleton<IVersionService, VersionsService>();
            services.AddSingleton<IMicrosoftService, MicrosoftService>();
            services.AddSingleton<ILauncherService, LauncherService>();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<NewProfileViewModel>();
            services.AddTransient<ProfileSettingsViewModel>();
            services.AddTransient<NewProfileViewModel>();
            services.AddTransient<AddServerViewModel>();

            services.AddTransient<ServerCardViewModel>();
            services.AddTransient<ProfileDetailsViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<NewProfileView>();
            services.AddTransient<ProfileSettingsView>();
            services.AddTransient<NewProfileView>();
            services.AddTransient<AddServerView>();

        }
    }

}
