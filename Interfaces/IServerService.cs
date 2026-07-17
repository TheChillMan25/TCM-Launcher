using MineStatLib;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Interfaces
{
    public interface IServerService
    {
        Task<Server?> AddServer(string serverName, string address, string mcVersion, string? pId = null);
        Task<List<Server>> GetSavedServersAsync();
        Task<Server> UpdateServer(Server server);
        Task<bool> DeleteServer(string id);
        Task<MineStat> FetchServerInfoAsync(string address);

    }
}
