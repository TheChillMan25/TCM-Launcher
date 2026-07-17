using CmlLib.Core;
using TCM_Launcher.Model.DB;

namespace TCM_Launcher.Interfaces
{
    public interface ILauncherService
    {
        Task<string> CreateProfileAsync(string pName, string mcVersion, string fVersion, IProgress<double> progress = null);
        Task LaunchProfileAsync(GameProfile profile, string? serverAddress = null);
    }
}
