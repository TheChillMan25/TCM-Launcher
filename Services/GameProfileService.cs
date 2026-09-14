using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Services
{
    public class GameProfileService : IGameProfileService
    {
        public async Task<GameProfile> AddProfileAsync(string name, string mcVersion, string fVersion, string? modpackId = null)
        {
            try
            {
                using var db = new LauncherDBContext();
                var newProfile = new GameProfile
                {
                    ProfileName = name,
                    MCVersion = mcVersion,
                    ForgeVersion = fVersion,
                    PackReleaseNumber = 1,
                    PlayTime = 0
                };

                db.GameProfiles.Add(newProfile);
                await db.SaveChangesAsync();
                return newProfile;
            }
            catch(Exception ex)
            {
                Logger.Error($"There was an exception thrown during adding profile to DB: ProfileName={name}, MCVersion={mcVersion}, ForgeVersion={fVersion}, ModpackId={modpackId}", ex);
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

        public async Task<List<GameProfile>> GetProfilesWithVersionAsync(string version)
        {
            try
            {
                using var db = new LauncherDBContext();
                return await db.GameProfiles.Where(p => p.MCVersion == version).ToListAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception thrown during reading all profiles", ex);
                MessageBox.Show("An error occured retrieving profile informations.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }

        }

        public async Task<GameProfile?> GetProfileAsync(string profileId)
        {
            try
            {
                using var db = new LauncherDBContext();
                return await db.GameProfiles.FirstOrDefaultAsync(p => p.Id == profileId);
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception thrown during reading profile with id: {profileId}", ex);
                MessageBox.Show("An error occured retrieving profile information.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        

        public async Task<GameProfile?> UpdateProfileAsync(GameProfile updateData)
        {
            try
            {
                using var db = new LauncherDBContext();

                var profile = await db.GameProfiles.FirstOrDefaultAsync(p => p.Id == updateData.Id);

                if (profile != null)
                {
                    profile.ProfileName = CheckUpdateData(updateData.ProfileName, profile.ProfileName);
                    profile.MCVersion = CheckUpdateData(updateData.MCVersion, profile.MCVersion);
                    profile.ForgeVersion = CheckUpdateData(updateData.ForgeVersion, profile.ForgeVersion);
                    profile.FileName = CheckUpdateData(updateData.FileName, profile.FileName);
                    profile.Installed = CheckUpdateData(updateData.Installed, profile.Installed);
                    profile.ModpackId = CheckUpdateData(updateData.ModpackId, profile.ModpackId);
                    profile.PlayTime = CheckUpdateData(updateData.PlayTime, profile.PlayTime);
                    profile.PackReleaseNumber = CheckUpdateData(updateData.PackReleaseNumber, profile.PackReleaseNumber);

                    await db.SaveChangesAsync();
                }

                return profile;
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception thrown during updating profile: {updateData.Id}, Data: ProfileName={updateData.ProfileName}, MCVersion={updateData.MCVersion}, ForgeVersion={updateData.ForgeVersion}, FileName={updateData.FileName}, Installed={updateData.Installed}", ex);
                MessageBox.Show("An error occured during updation the profle.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private T CheckUpdateData<T>(T updateData, T oldData)
        {
            if (updateData != null && !EqualityComparer<T>.Default.Equals(updateData, oldData)) return updateData;
            return oldData;
        }

        public async Task<bool> DeleteProfileAsync(string profileId)
        {
            try
            {
                using var db = new LauncherDBContext();

                var serversToUnbind = await db.SavedServers.Where(s => s.BindedProfileId == profileId).ToListAsync();
                foreach (var server in serversToUnbind)
                {
                    server.BindedProfileId = null;
                }

                var settings = await db.ProfileSettings.FirstOrDefaultAsync(ps => ps.GameProfileId == profileId);
                if (settings != null)
                {
                    db.ProfileSettings.Remove(settings);
                }

                var profile = await db.GameProfiles.FirstOrDefaultAsync(p => p.Id == profileId);

                if (profile != null)
                {
                    db.GameProfiles.Remove(profile);
                    await db.SaveChangesAsync();
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
