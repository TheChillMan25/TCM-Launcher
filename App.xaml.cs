using System.Windows;
using TCM_Launcher.Core.DBContext;

namespace TCM_Launcher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var db = new LauncherDBContext();

            db.Database.EnsureCreated();
        }
    }

}
