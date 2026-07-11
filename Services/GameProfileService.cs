using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Core.Utils;
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
            }
            catch(Exception ex)
            {
                Logger.Error($"There was an exception thrown during adding profile to DB: ProfileName={name}, MCVersion={mcVersion}, ForgeVersion={fVersion}, FileName={fileName}", ex);
                MessageBox.Show("An error occured during profile creation.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            
        }

        public async Task<List<GameProfile>> GetAllGameProfiles()
        {
            try
            {
                using var db = new LauncherDBContext();
                return await db.GameProfiles.ToListAsync();
            }
            catch(Exception ex)
            {
                Logger.Error("There was an exception thrown during reading all profiles", ex);
                MessageBox.Show("An error occured retrieving profile informations.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Logger.Error($"There was an exception thrown during reading profile with id: {profileId}", ex);
                MessageBox.Show("An error occured retrieving profile information.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    profile.ProfileName = updateData.ProfileName == null ? profile.ProfileName : updateData.ProfileName;
                    profile.MCVersion = updateData.MCVersion == null ? profile.MCVersion: updateData.MCVersion; ;
                    profile.ForgeVersion = updateData.ForgeVersion == null ? profile.ForgeVersion : updateData.ForgeVersion;
                    profile.FileName = updateData.FileName == null ? profile.FileName : updateData.FileName;
                    profile.Installed = updateData.Installed == null ? profile.Installed : updateData.Installed;

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception thrown during updating profile: {profileId}, Data: ProfileName={updateData.ProfileName}, MCVersion={updateData.MCVersion}, ForgeVersion={updateData.ForgeVersion}, FileName={updateData.FileName}, Installed={updateData.Installed}", ex);
                MessageBox.Show("An error occured during updation the profle.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateLastPlayedProfile(string profileId)
        {
            GameProfile? profile = null;
            GameProfile? oldProfile = null;
            try
            {
                using var db = new LauncherDBContext();

                profile = db.GameProfiles.FirstOrDefault(p => p.Id == profileId);
                oldProfile = db.GameProfiles.FirstOrDefault(p => p.LastPlayed == true);

                if (oldProfile != null) oldProfile.LastPlayed = false;
                if (profile != null) profile.LastPlayed = true;

                db.SaveChanges();


            }
            catch (Exception ex)
            {
                string oldId = oldProfile != null ? oldProfile.Id : "NULL (Old not found)";
                string foundId = profile != null ? profile.Id : "NULL (New not found)";
                Logger.Error($"There was an exception thrown during updating last played profile with id: {profileId}\nData: New id={foundId}, Old id={oldId}", ex);
                MessageBox.Show("An error occured updating lastly played profile information.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Logger.Error($"There was an exception thrown during deleting profile with id: {profileId}", ex);
                MessageBox.Show("An error occured during profile deletion.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void OpenProfileFolder(string profileId, string subFolder = "")
        {
            try
            {
                string profileFolder = Path.Combine(Constants.ProfilesPath, profileId, subFolder);
                if (Directory.Exists(profileFolder))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = profileFolder,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                string msg = string.IsNullOrEmpty(subFolder) ? profileId : profileId + $" (Subfolder={subFolder}";
                Logger.Error($"There was an exception while opening profile folder with id: {msg}", ex);
                MessageBox.Show("An error occured during opening profile folder", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
