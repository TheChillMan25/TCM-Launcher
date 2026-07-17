using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Interfaces
{
    public interface IProfileSettingsService
    {
        Task<ProfileSettings> SetProfileSettingsAsync(ProfileSettings data);
        Task<ProfileSettings> GetProfileSettings(string profileId);
    }
}
