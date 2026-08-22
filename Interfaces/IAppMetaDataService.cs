using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Interfaces
{
    public interface IAppMetaDataService
    {
        Task<AppMetaData?> AddMetaData(string key, string value);
        Task<string?> GetMetaData(string key);
        Task<AppMetaData?> UpdateMetaData(string key, string value);
        Task<bool> DeleteMetaData(string key);
    }
}
