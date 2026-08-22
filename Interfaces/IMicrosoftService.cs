using CmlLib.Core.Auth;

namespace TCM_Launcher.Interfaces
{
    public interface IMicrosoftService
    {
        MSession MSession { get; set; }
        Task<MSession> MicrosoftLoginAsync(bool silent = false);
        Task<bool> MicrosoftSignOutAsync();
    }
}
