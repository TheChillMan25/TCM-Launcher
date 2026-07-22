using TCM_Launcher.Model.Mods;
using TCML_Class_library;

namespace TCM_Launcher.Interfaces
{
    public interface IProfileModService
    {
        Task<ProfileModpackManifest> LoadManifestAsync(string profileId);
        Task SaveManifestAsync(string profileId, ProfileModpackManifest manifest);
        Task ToggleModAsync(string profileId, ProfileModInfo mod, bool enable);
        Task SyncProfileModsAsync(string profileId);
        Task AddModWithDependenciesAsync(string profileId, string mcVersion, ModDetails modDetails, ModVersion selectedVersion);
        Task<bool> IsVersionBindedToProfileAsync(string profileId, ModVersion version);
        Task<string?> RemoveModFromProfile(string profileId, string projectId);
    }
}
