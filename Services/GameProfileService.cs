using System.Windows;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Services
{
    public class GameProfileService
    {
        public static GameProfileService Instance { get; } = new GameProfileService();
        public GameProfile AddProfile(string name, string mcVersion, string fVersion, string? fileName = null)
        {
            try
            {
                using var db = new LauncherDBContext();
                var newProfile = new GameProfile
                {
                    ProfileName = name,
                    MCVersion = mcVersion,
                    ForgeVersion = fVersion,
                    FileName = fileName,
                    Installed = false,
                };

                db.GameProfiles.Add(newProfile);
                db.SaveChanges();
                return newProfile;
            }catch(Exception ex)
            {
                MessageBox.Show("An error occured during profile creation.");
                return null;
            }
            
        }

        public List<GameProfile> GetAllGameProfiles()
        {
            try
            {
                using var db = new LauncherDBContext();
                return db.GameProfiles.ToList();
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occured retrieving profile informations.");
                return [];
            }
            
        }

        public GameProfile GetProfile(string profileId)
        {
            try
            {
                using var db = new LauncherDBContext();
                return db.GameProfiles.FirstOrDefault(p => p.Id == profileId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured retrieving profile information.");
                return null;
            }
        }

        public GameProfile GetLastPlayedProfile()
        {
            try
            {
                using var db = new LauncherDBContext();
                return db.GameProfiles.FirstOrDefault(p => p.LastPlayed == true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured retrieving lastly played profile information.");
                return null;
            }
        }

        public void UpdateProfile(string profileId, GameProfile updateData)
        {
            try
            {
                using var db = new LauncherDBContext();

                var profile = db.GameProfiles.FirstOrDefault(p => p.Id == profileId);

                if (profile != null)
                {
                    profile.ProfileName = updateData.ProfileName;
                    profile.MCVersion = updateData.MCVersion;
                    profile.ForgeVersion = updateData.ForgeVersion;
                    profile.FileName = updateData.FileName;
                    profile.Installed = updateData.Installed;

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured during updation the profle.");
            }
        }

        public void UpdateLastPlayedProfile(string profileId)
        {
            try
            {
                using var db = new LauncherDBContext();

                var profile = db.GameProfiles.FirstOrDefault(p => p.Id == profileId);
                var oldProfile = db.GameProfiles.FirstOrDefault(p => p.LastPlayed == true);

                if (oldProfile != null)
                {
                    oldProfile.LastPlayed = false;
                    if (profile != null) profile.LastPlayed = true;
                    db.SaveChanges();
                }

            
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured updating lastly played profile information.");
            }
        }

        public bool DeleteProfile(string profileId)
        {
            try
            {
                using var db = new LauncherDBContext();

                var profile = db.GameProfiles.FirstOrDefault(p => p.Id == profileId);

                if (profile != null)
                {
                    db.GameProfiles.Remove(profile);
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured during profile deletion.");
                return false;
            }
        }
    }
}
