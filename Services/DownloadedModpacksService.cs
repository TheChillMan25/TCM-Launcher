using Microsoft.EntityFrameworkCore;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCML_Class_library;

namespace TCM_Launcher.Services
{
    public class DownloadedModpacksService : IDownloadedModpacksService
    {
        public async Task<FirestoreModpack?> AddModpackAsync(FirestoreModpack modpack)
        {
            try
            {
                using var db = new LauncherDBContext();
                var existing = await db.DownloadedModpacks.FirstOrDefaultAsync(m => m.Id == modpack.Id);
                if (existing != null) return null;
                await db.AddAsync(modpack);
                await db.SaveChangesAsync();
                return modpack;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when adding modpack.", ex);
                return null;
            }
        }

        public async Task<bool?> DeleteModpackAsync(string id)
        {
            try
            {
                using var db = new LauncherDBContext();
                var existing = await db.DownloadedModpacks.FirstOrDefaultAsync(m => m.Id == id);
                if (existing != null)
                {
                    await db.GameProfiles.Where(p => p.ModpackId == id)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.ModpackId, (string?)null));
                    db.DownloadedModpacks.Remove(existing);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when adding modpack.", ex);
                return null;
            }
        }

        public async Task<FirestoreModpack?> GetModpackAsync(string id)
        {
            try
            {
                using var db = new LauncherDBContext();
                return await db.DownloadedModpacks.FirstOrDefaultAsync(m  => m.Id == id);
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when adding modpack.", ex);
                return null;
            }
        }

        public async Task<bool> IsExistingModpackAsync(string? modpackId)
        {
            try
            {
                using var db = new LauncherDBContext();
                var existing = await db.DownloadedModpacks.FirstOrDefaultAsync(m => m.Id == modpackId);
                return existing != null;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when checking for existing modpack.", ex);
                return false;
            }
        }

        public async Task<FirestoreModpack?> UpdateModpackAsync(FirestoreModpack updateData)
        {
            try
            {
                using var db = new LauncherDBContext();
                var existing = db.DownloadedModpacks.FirstOrDefault(m => m.Id == updateData.Id);
                if (existing != null)
                {
                    existing.ReleaseNumber = updateData.ReleaseNumber;
                    await db.SaveChangesAsync();
                    return existing;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when adding modpack.", ex);
                return null;
            }
        }
    }
}
