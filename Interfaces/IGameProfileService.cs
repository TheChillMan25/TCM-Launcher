using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Interfaces
{
    public interface IGameProfileService
    {
        Task<GameProfile> AddProfileAsync(string name, string mcVersion, string fVersion, string? fileName = null);
        Task<List<GameProfile>> GetAllGameProfiles();
        Task<List<GameProfile>> GetProfilesWithVersionAsync(string version);
        Task<List<GameProfile>> GetPinnedProfilesAsync();
        Task<GameProfile?> GetProfileAsync(string profileId);
        Task<GameProfile?> UpdateProfileAsync(string profileId, GameProfile updateData);
        Task<GameProfile> UpdateLastPlayedProfileAsync(string profileId);
        Task<bool> DeleteProfileAsync(string profileId);
        void OpenProfileFolder(string profileId, string subFolder = "");
    }
}
