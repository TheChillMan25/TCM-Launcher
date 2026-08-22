using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.Mods;
using TCML_Class_library;

namespace TCM_Launcher.Interfaces
{
    public interface IProfileModService
    {
        Task CreateProfileManifest(string profileId, string profileName, string mcVersion, string forgeVersion);
        Task<ProfileModpackManifest> LoadManifestAsync(string profileId);
        Task SaveManifestAsync(string profileId, ProfileModpackManifest manifest);
        Task ToggleModAsync(string profileId, ProfileModInfo mod, bool enable);
        Task<bool> SyncProfileModsAsync(string profileId, IProgress<double>? progress = null, IProgress<string>? status = null, string? serverAddress = null);
        Task<List<ProfileModInfo>> AddModWithDependenciesAsync(string profileId, string mcVersion, ModDetails modDetails, ModVersion selectedVersion);
        Task<bool> IsVersionBindedToProfileAsync(string profileId, ModVersion version);
        Task<string?> RemoveModFromProfile(string profileId, string projectId);
        Task ExportModpackAsync(string profileId, string profileName, bool exportModJARs = true, List<ProfileModInfo>? mods = null, bool exportOnlyImportedFiles = false);
        Task ImportModpackAsync(string profileId, string? source = null);
        Task<ProfileModInfo?> ImportModAsync(string profileId, string modName, string modVersion, string fileName, string sourceFile, string clientSide, string serverSide, bool missingJar = false);
        Task<GameProfile?> ImportModpackDirectlyAsync(string filePath);
        Task UpdateModAsync(string profielId, ProfileModInfo mod);
    }
}
