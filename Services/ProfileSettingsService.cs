using System.Windows;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Services
{
    public class ProfileSettingsService
    {
        public static ProfileSettingsService Instance { get; set; } = new ProfileSettingsService();

        public ProfileSettings SetProfileSettings(ProfileSettings data)
        {
            try
            {
                using var db = new LauncherDBContext();
                var settings = db.ProfileSettings.FirstOrDefault(s => s.GameProfileId == data.GameProfileId);

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
                db.SaveChanges();
                return settings;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("An error occured during setting profile settings.");
                return null;
            }
        }

        public ProfileSettings GetProfileSettings(string profileId)
        {
            try
            {
                using var db = new LauncherDBContext();
                var s = db.ProfileSettings.FirstOrDefault(s => s.GameProfileId == profileId);
                if (s != null)
                {
                    return s;
                }
                MessageBox.Show("There are no settings for this profile.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("An error occured during loading profile settings.");
                return null;
            }
        }
    }
}
