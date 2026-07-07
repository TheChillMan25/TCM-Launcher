using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.Installers;
using CmlLib.Core.ProcessBuilder;
using System.IO;
using TCM_Launcher.Core;
using TCM_Launcher.Model.UI;

namespace TCM_Launcher.Model
{
    class MCLauncher
    {
        MinecraftLauncher launcher;
        ForgeInstaller fInstaller;
        private string latestForgeInstallName;

        public string LatestForgeInstallName
        {
            get { return latestForgeInstallName; }
            set { latestForgeInstallName = value; }
        }

        public async Task StartGame()
        {
            try
            {
                var gameProcess = await launcher.BuildProcessAsync(LatestForgeInstallName, new MLaunchOption
                {
                    MaximumRamMb = 4096,
                    MinimumRamMb = 4096,
                    Session = MSession.CreateOfflineSession("Gamer123"),
                });
                gameProcess.StartInfo.UseShellExecute = false;
                gameProcess.StartInfo.RedirectStandardError = true;
                gameProcess.StartInfo.RedirectStandardOutput = true;

                gameProcess.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine($"[MC ERROR] {e.Data}"); };
                gameProcess.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine($"[MC LOG] {e.Data}"); };

                gameProcess.Start();

                gameProcess.BeginErrorReadLine();
                gameProcess.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
