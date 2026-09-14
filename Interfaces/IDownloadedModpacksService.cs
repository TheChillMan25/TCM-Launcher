using TCML_Class_library;

namespace TCM_Launcher.Interfaces
{
    public interface IDownloadedModpacksService
    {
        Task<FirestoreModpack?> AddModpackAsync(FirestoreModpack modpack);
        Task<FirestoreModpack?> UpdateModpackAsync(FirestoreModpack updateData);
        Task<FirestoreModpack?> GetModpackAsync(string id);
        Task<bool?> DeleteModpackAsync(string id);
        Task<bool> IsExistingModpackAsync(string? modpackId);
    }
}
