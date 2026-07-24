using CmlLib.Core;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Interfaces
{
    public interface ILauncherService
    {
        Task<string> CreateProfileAsync(string pName, string mcVersion, string fVersion, IProgress<double> progress, IProgress<string> status, IProgress<bool> progressVisible);
        Task LaunchProfileAsync(GameProfile profile, IProgress<double> progress, IProgress<string> status, IProgress<bool> progressVisible, string? serverAddress = null);
    }
}
