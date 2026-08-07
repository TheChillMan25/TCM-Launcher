using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private const string AppRegistryName = "TCM_Launcher";
        public AppSettings AppSettings { get; private set; }

        public Task LoadTask { get; private set; }

        /// <summary>
        /// Closes, hides or keeps the launcher window open.
        /// </summary>
        /// <param name="behavior">The behaviour of the window on the current action</param>
        /// <param name="value">Additional visibility of the window (for example: hidden -> true/false)</param>
        public void LauncherWindowBehaviour(Constants.LauncherWindowBehaviour behavior, bool? value = null)
        {
            switch (behavior)
            {
                case Constants.LauncherWindowBehaviour.Minimize:
                    if (value == true) App.Current.MainWindow.Hide();
                    else if (value == false)
                    {
                        var window = App.Current.MainWindow;
                        window.Show();
                        window.WindowState = WindowState.Maximized;
                        window.Activate();
                    }
                    break;
                case Constants.LauncherWindowBehaviour.Close:
                    App.Current.Shutdown();
                    break;
            }
        }

        public async Task SaveSettings(AppSettings settings)
        {
            try
            {
                using var db = new LauncherDBContext();
                var s = await db.AppSettings.FirstOrDefaultAsync();
                if (s != null)
                {
                    if (s == settings) return;
                    s.StartWithWindows = settings.StartWithWindows;
                    s.CloseButtonBehaviour = settings.CloseButtonBehaviour;
                    s.OnGameStart = settings.OnGameStart;
                    s.MaximumParalellDownloads = settings.MaximumParalellDownloads;

                    await db.SaveChangesAsync();
                    SetAutoStart(s.StartWithWindows);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when saving app settings.", ex);
            }
        }

        public void StartLoadingSettings()
        {
            if (LoadTask == null ||  LoadTask.IsCompleted)
            {
                LoadTask = LoadSettings();
            }
        }

        private async Task LoadSettings()
        {
            try
            {
                using var db = new LauncherDBContext();
                var settings = await db.AppSettings.FirstOrDefaultAsync();
                if (settings == null)
                {
                    settings = new AppSettings();
                    await db.AppSettings.AddAsync(settings);
                    await db.SaveChangesAsync();
                }
                AppSettings = settings;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception while loading app settings", ex);
            }
        }

        public void SetAutoStart(bool autostart)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: true);

                if (key == null) return;

                if (autostart)
                {
                    string? exePath = Environment.ProcessPath;

                    if(!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppRegistryName, $"\"{exePath}\"");
                    }
                }
                else
                {
                    if(key.GetValue(AppRegistryName) != null)
                    {
                        key.DeleteValue(AppRegistryName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an error when setting start with windows.", ex);
            }
        }

        public bool IsAutoStartEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: false);

                return key?.GetValue(AppRegistryName) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
