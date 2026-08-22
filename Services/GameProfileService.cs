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
        public async Task<GameProfile> AddProfileAsync(string name, string mcVersion, string fVersion, string? fileName = null)
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
                };

                db.GameProfiles.Add(newProfile);
                await db.SaveChangesAsync();
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

        public async Task<List<GameProfile>> GetPinnedProfilesAsync()
        {
            try
            {
                using var db = new LauncherDBContext();
                return db.GameProfiles.Where(p => p.Pinned == true).ToList() ?? new List<GameProfile>();
            }
            catch(Exception ex)
            {
                Logger.Error("There was an exception when fetching pinned profiles.", ex);
                return new List<GameProfile>();
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

        public async Task<GameProfile?> UpdateProfileAsync(string profileId, GameProfile updateData)
        {
            try
            {
                using var db = new LauncherDBContext();

                var profile = await db.GameProfiles.FirstOrDefaultAsync(p => p.Id == profileId);

                if (profile != null)
                {
                    profile.ProfileName = CheckUpdateData(updateData.ProfileName, profile.ProfileName);
                    profile.MCVersion = CheckUpdateData(updateData.MCVersion, profile.MCVersion);
                    profile.ForgeVersion = CheckUpdateData(updateData.ForgeVersion, profile.ForgeVersion);
                    profile.FileName = CheckUpdateData(updateData.FileName, profile.FileName);
                    profile.Installed = CheckUpdateData(updateData.Installed, profile.Installed);
                    profile.Pinned = CheckUpdateData(updateData.Pinned, profile.Pinned);

                    await db.SaveChangesAsync();
                }

                return profile;
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception thrown during updating profile: {profileId}, Data: ProfileName={updateData.ProfileName}, MCVersion={updateData.MCVersion}, ForgeVersion={updateData.ForgeVersion}, FileName={updateData.FileName}, Installed={updateData.Installed}", ex);
                MessageBox.Show("An error occured during updation the profle.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private T CheckUpdateData<T>(T updateData, T oldData)
        {
            if (updateData != null && !EqualityComparer<T>.Default.Equals(updateData, oldData)) return updateData;
            return oldData;
        }

        public async Task<GameProfile> UpdateLastPlayedProfileAsync(string profileId)
        {
            GameProfile? profile = null;
            GameProfile? oldProfile = null;
            try
            {
                using var db = new LauncherDBContext();

                profile = await db.GameProfiles.FirstOrDefaultAsync(p => p.Id == profileId);
                oldProfile = await db.GameProfiles.FirstOrDefaultAsync(p => p.LastPlayed == true);

                if (oldProfile != null) oldProfile.LastPlayed = false;
                if (profile != null) profile.LastPlayed = true;

                await db.SaveChangesAsync();
                return profile;
            }
            catch (Exception ex)
            {
                string oldId = oldProfile != null ? oldProfile.Id : "NULL (Old not found)";
                string foundId = profile != null ? profile.Id : "NULL (New not found)";
                Logger.Error($"There was an exception thrown during updating last played profile with id: {profileId}\nData: New id={foundId}, Old id={oldId}", ex);
                MessageBox.Show("An error occured updating lastly played profile information.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
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
