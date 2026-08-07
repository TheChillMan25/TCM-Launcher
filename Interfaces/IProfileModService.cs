using TCM_Launcher.Model.Mods;
using TCML_Class_library;

namespace TCM_Launcher.Interfaces
{
    public interface IProfileModService
    {
        Task<ProfileModpackManifest> LoadManifestAsync(string profileId);
        Task SaveManifestAsync(string profileId, ProfileModpackManifest manifest);
        Task ToggleModAsync(string profileId, ProfileModInfo mod, bool enable);
        Task<bool> SyncProfileModsAsync(string profileId, IProgress<double>? progress = null, IProgress<string>? status = null, string? serverAddress = null);
        Task<List<ProfileModInfo>> AddModWithDependenciesAsync(string profileId, string mcVersion, ModDetails modDetails, ModVersion selectedVersion);
        Task<bool> IsVersionBindedToProfileAsync(string profileId, ModVersion version);
        Task<string?> RemoveModFromProfile(string profileId, string projectId);
        Task ExportModpackAsync(string profileId, string profileName);
        Task ImportModpackAsync(string profileId);
        Task<ProfileModInfo?> ImportModAsync(string profileId, string modName, string modVersion, string fileName, string sourceFile, bool clientSide, bool serverSide, bool missingJar = false);
        //Task ToggleQuickJoinServerMods(string profileId, string modsFolder, bool enabled, ProfileModpackManifest? manifest = null);
    }
}
