using Microsoft.EntityFrameworkCore;
using System.Windows;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Services
{
    public class AppMetaDataService : IAppMetaDataService
    {
        public async Task<bool> DeleteMetaData(string key)
        {
            try
            {
                using var db = new LauncherDBContext();
                var data = await db.AppMetaData.FirstOrDefaultAsync(d => d.Key == key);
                if (data != null)
                {
                    db.Remove(data);
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                Logger.Error("There was an exception when deleting meta data.", ex);
                return false;
            }
        }

        public async Task<string?> GetMetaData(string key)
        {
            try
            {
                using var db = new LauncherDBContext();
                var data = await db.AppMetaData.FirstOrDefaultAsync(d => d.Key == key);
                return data?.Value;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when adding meta data.", ex);
                return null;
            }
        }

        public async Task<AppMetaData?> AddMetaData(string key, string value)
        {
            try
            {
                using var db = new LauncherDBContext();
                var data = new AppMetaData
                {
                    Key = key,
                    Value = value,
                    UpdatedAt = DateTime.Now,
                };
                await db.AppMetaData.AddAsync(data);
                await db.SaveChangesAsync();
                return data;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when adding meta data.", ex);
                return null;
            }
        }

        public async Task<AppMetaData?> UpdateMetaData(string key, string value)
        {
            try
            {
                using var db = new LauncherDBContext();
                var data = await db.AppMetaData.FirstOrDefaultAsync(x => x.Key == key);
                if (data != null)
                {
                    data.Value = value;
                    await db.SaveChangesAsync();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when updating meta data.", ex);
                return null;
            }
        }
    }
}
