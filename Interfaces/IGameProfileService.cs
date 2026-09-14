using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Interfaces
{
    public interface IGameProfileService
    {
        Task<GameProfile> AddProfileAsync(string name, string mcVersion, string fVersion, string? modpackId = null);
        Task<List<GameProfile>> GetAllGameProfiles();
        Task<List<GameProfile>> GetProfilesWithVersionAsync(string version);
        Task<GameProfile?> GetProfileAsync(string profileId);
        Task<GameProfile?> UpdateProfileAsync(GameProfile updateData);
        Task<bool> DeleteProfileAsync(string profileId);
        void OpenProfileFolder(string profileId, string subFolder = "");
    }
}
