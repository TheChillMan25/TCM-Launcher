using TCM_Launcher.Model.DB;
using TCM_Launcher.Model.Mods;
using TCML_Class_library;

namespace TCM_Launcher.Interfaces
{
    public interface IProfileModService
    {
        public event Action<string>? OnUpdatedModpack;
        Task CreateProfileManifest(string profileId, string profileName, string mcVersion, string forgeVersion, string modpackId);
        Task<ProfileModpackManifest> LoadManifestAsync(string profileId);
        Task SaveManifestAsync(string profileId, ProfileModpackManifest manifest);
        Task ToggleModAsync(string profileId, ProfileModInfo mod, bool enable);
        Task<bool> SyncProfileModsAsync(string profileId, IProgress<double>? progress = null, IProgress<string>? status = null, string? serverAddress = null);
        Task<List<ProfileModInfo>> AddModWithDependenciesAsync(string profileId, string mcVersion, ModDetails modDetails, ModVersion selectedVersion);
        Task<bool> IsVersionBindedToProfileAsync(string profileId, ModVersion version);
        Task<string?> RemoveModFromProfile(string profileId, string projectId);
        Task<string?> ExportModpackAsync(string profileId, string profileName, bool exportModJARs = true, List<ProfileModInfo>? mods = null, bool exportOnlyImportedFiles = false);
        Task ImportModpackAsync(string profileId, string? source = null);
        Task<ProfileModInfo?> ImportModAsync(string profileId, string modName, string modVersion, string fileName, string sourceFile, string clientSide, string serverSide, bool missingJar = false);
        Task<GameProfile?> ImportModpackDirectlyAsync(string filePath, bool importFromStorage = false, FirestoreModpack? modpack = null, bool update = false, GameProfile? profileToUpdate = null);
        Task UpdateModAsync(string profielId, ProfileModInfo mod);
    }
}
