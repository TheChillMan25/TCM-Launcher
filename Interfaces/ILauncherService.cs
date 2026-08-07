using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Interfaces
{
    public interface ILauncherService
    {
        Task<string> CreateProfileAsync(string pName, string mcVersion, string fVersion, IProgress<double>? progress = null, IProgress<string>? status = null, IProgress<bool>? progressVisible = null);
        Task LaunchProfileAsync(GameProfile profile, IProgress<double>? progress = null, IProgress<string>? status = null, IProgress<bool>? progressVisible = null, string? serverAddress = null);
    }
}
