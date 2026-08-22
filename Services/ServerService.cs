using Microsoft.EntityFrameworkCore;
using MineStatLib;
using TCM_Launcher.Core.DBContext;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Services
{
    public class ServerService : IServerService

    {

        public async Task<Server?> AddServer(string serverName, string address, string mcVersion, string? pId = null)
        {
            try
            {
                using var db = new LauncherDBContext();

                var server = new Server
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = serverName,
                    Address = address,
                    MCVersion = mcVersion,
                    BindedProfileId = pId
                };

                db.SavedServers.Add(server);
                await db.SaveChangesAsync();
                return server;
            }
            catch(Exception ex)
            {
                Logger.Error($"There was an exception during adding server.\nData:\nServer name={serverName}\nIP={address}\nMCVersion={mcVersion}\nProfiel ID={pId}", ex);
                return null;
            }
        }

        public async Task<List<Server>> GetSavedServersAsync()
        {
            try
            {
                using var db = new LauncherDBContext();
                return await db.SavedServers.ToListAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("There was an eception during fetching saved servers.", ex);
                return [];
            }
        }

        public async Task<Server> UpdateServer(Server server)
        {
            try
            {
                using var db = new LauncherDBContext();

                var s = await db.SavedServers.FirstOrDefaultAsync(s => s.Id == server.Id);
                if(s != null)
                {
                    s.Name = server.Name;
                    s.Address = server.Address;
                    s.MCVersion = server.MCVersion;
                    s.BindedProfileId = server.BindedProfileId;

                    await db.SaveChangesAsync();
                    return s;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception when updating server with id: {server.Id}\nData:\nServerName={server.Name}\nServerAddress={server.Address}\nServerVersion={server.MCVersion}\nBindedProfile={server.BindedProfile}", ex);
                return null;
            }
        }

        public async Task<bool> DeleteServer(string id)
        {
            try
            {
                using var db = new LauncherDBContext();
                var server = await db.SavedServers.FirstOrDefaultAsync(s => s.Id == id);
                
                if(server != null)
                {
                    db.SavedServers.Remove(server);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception when deleting profile with id: {id}");
                return false;
            }
        }
        public async Task<MineStat> FetchServerInfoAsync(string address)
        {
            if (address == null) return null;
            try
            {
                string ip = address;
                ushort port = 25565;

                if (ip.Contains(":"))
                {
                    var parts = ip.Split(":");
                    ip = parts[0];
                    ushort.TryParse(parts[1], out port);
                }

                var ms = await Task.Run(() => new MineStat(ip, port));

                return ms;
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception when pinging minecraft server with address: {address}.", ex);
                return null;
            }
        }
    }
}
