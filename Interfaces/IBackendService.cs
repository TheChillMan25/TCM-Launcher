using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
using TCML_Class_library;

namespace TCM_Launcher.Interfaces
{
    public interface IBackendService
    {
        public Func<string,bool,FirestoreModpack, bool, GameProfile?, Task> OnModpackDownloadedFromStorage { get; set; }
        Task<List<ModSearchResult>> SearchModsAsync(string query, string mcVersion, CancellationToken cToken = default);
        Task<ModDetails?> GetModDetailsAsync(string projectId, string modrinthId, string curseforgeId, string mcVersion, ModSource source, bool needDesc = true);
        Task<FirestoreModpack?> UploadModpackAsync(string filePath, string name, string version, string? modpackId, List<string> sharedWith, bool isUpdate = false);
        Task<FirebaseTokenData?> GetCustomTokenAsync();
        Task<List<FirestoreUser>?> SearchFriendsAsync(string username);
        Task<FirestoreRequest?> AddFriendAsync(string uuid);
        Task UpdateFriendRequestAsync(string requestId, bool value);
        Task RemoveFriendAsync(string uuid);
        Task DownloadModpackAsync(FirestoreModpack modpack, bool isUpdate = false, GameProfile? profileToUpdate = null);
        Task DeleteModpackAsync(FirestoreModpack modpack);
        Task<bool> ShareModpackAsync(FirestoreModpack modpack, List<string> selected);
        Task<FirebaseTokenResponse?> RefreshFirebaseTokenAsync(string? refreshToken);
    }
}
