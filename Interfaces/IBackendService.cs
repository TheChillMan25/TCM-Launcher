using TCML_Class_library;

namespace TCM_Launcher.Interfaces
{
    public interface IBackendService
    {
        Task<List<ModSearchResult>> SearchModsAsync(string query, string mcVersion, CancellationToken cToken = default);
        Task<ModDetails?> GetModDetailsAsync(string projectId, string mcVersion, ModSource source, bool needDesc = true);
    }
}
