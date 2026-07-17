using TCM_Launcher.Model.DB.Versions;

namespace TCM_Launcher.Interfaces
{
    public interface IVersionService
    {
        Task SyncTask { get; }
        void StartVersionCheck();
        Task<List<VanillaVersion>> GetVanillaVersions();
        Task<List<ForgeVersion>> GetForgeVersions(string mcVersion);
    }
}
