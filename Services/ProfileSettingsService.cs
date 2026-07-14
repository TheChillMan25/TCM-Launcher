using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Interop;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Services
{
    public class ProfileSettingsService
    {
        public static ProfileSettingsService Instance { get; set; } = new ProfileSettingsService();

        public async Task<ProfileSettings> SetProfileSettingsAsync(ProfileSettings data)
        {
            ProfileSettings settings = null;
            try
            {
                using var db = new LauncherDBContext();
                settings = await db.ProfileSettings.FirstOrDefaultAsync(s => s.GameProfileId == data.GameProfileId);

                if (settings != null)
                {
                    settings.Ram = data.Ram;
                    settings.JVMArgs = data.JVMArgs;
                }
                else
                {
                    settings = new ProfileSettings
                    {
                        GameProfileId = data.GameProfileId,
                        Ram = data.Ram,
                        JVMArgs = data.JVMArgs
                    };
                    db.ProfileSettings.Add(settings);
                }
                await db.SaveChangesAsync();
                return settings;
            }
            catch(Exception ex)
            {
                string msg = settings == null ? "NULL" : settings.GameProfileId;
                Logger.Error($"There was an exception during setting profile settings with ProfileId={msg}", ex);
                MessageBox.Show("An error occured during setting profile settings.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<ProfileSettings> GetProfileSettings(string profileId)
        {
            try
            {
                using var db = new LauncherDBContext();
                var s = await db.ProfileSettings.FirstOrDefaultAsync(s => s.GameProfileId == profileId);
                if (s != null)
                {
                    return s;
                }
                MessageBox.Show("There are no settings for this profile.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            catch (Exception ex)
            {
                string msg = profileId == null ? "NULL" : profileId;
                Logger.Error($"There was an exception during reading profile settings with ProfileId={msg}", ex);
                MessageBox.Show("An error occured during loading profile settings.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
