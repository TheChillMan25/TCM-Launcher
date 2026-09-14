using TCML_Class_library;

namespace TCM_Launcher.Interfaces
{
    public interface IFirebaseService
    {
        public List<FirestoreUser> CachedFriends { get; }
        public List<FirestoreModpack> CachedModpacks { get; }
        Task<string?> GetAuthTokenAsync();
        Task ListenToRequestsAsync(string uuid, Action<FirestoreRequest> onAdded, Action<string> onRemoved);
        Task ListenToModpacksAsync(string uuid, Action<List<FirestoreModpack>> onChange);
        Task ListenToFriendsList(string uuid, Action<List<FirestoreUser>> onChange);
        Task StopListeningAsync();
    }
}
