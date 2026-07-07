using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.ProcessBuilder;
using System.IO;
using System.Net.Http;
using System.Windows;
using TCM_Launcher.Core;

namespace TCM_Launcher.Services
{
    public class LauncherService
    {
        public static LauncherService Instance { get; set; } = new LauncherService();

        public async Task<string> CreateProfileAsync(string pName, string mcVersion, string fVersion, IProgress<double> progress = null)
        {
            try
            {
                MinecraftPath path = CreateProfilePath(pName);
                var launcher = new MinecraftLauncher(path);
                launcher.ByteProgressChanged += (sender, args) =>
                {
                    if (args.TotalBytes > 0)
                    {
                        double percentage = (double)args.ProgressedBytes / args.TotalBytes * 100;
                        progress?.Report(percentage);
                    }
                    Console.WriteLine($"{args.ProgressedBytes} bytes / {args.TotalBytes} bytes");
                };
                return await InstallForgeAsync(launcher, mcVersion, fVersion, progress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task LaunchProfileAsync(string profileId, string fileName)
        {
            try
            {
                MinecraftPath path = CreateProfilePath(profileId);
                var launcher = new MinecraftLauncher(path);
                var profileSettings = ProfileSettingsService.Instance.GetProfileSettings(profileId);
                var jvmArgs = new List<MArgument>();
                if (!string.IsNullOrWhiteSpace(profileSettings.JVMArgs))
                {
                    var sArgs = profileSettings.JVMArgs?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
                    if (sArgs.Length > 0)
                    {
                        foreach (var s in sArgs)
                        {
                            jvmArgs.Add(new MArgument(s));
                        }
                    }
                }
                await launcher.InstallAsync(fileName);
                var process = await launcher.BuildProcessAsync(fileName, new MLaunchOption
                {
                    MaximumRamMb = profileSettings.Ram ?? Constants.DefaultRam,
                    Session = MSession.CreateOfflineSession("Gamer123"),
                    ExtraJvmArguments = jvmArgs,
                });
                var processWrapper = new ProcessWrapper(process);
                processWrapper.OutputReceived += (sender, log) =>
                {
                    Console.WriteLine(log);
                };
                
                GameProfileService.Instance.UpdateLastPlayedProfile(profileId);
                processWrapper.StartWithEvents();

                int exitCode = await processWrapper.WaitForExitTaskAsync();
                if (exitCode == 0)
                {
                    Console.WriteLine("Game terminated successfully.");
                }
                else
                {
                    Console.WriteLine($"Game error occurred! Exit code: {exitCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured during launching game.");
            }
            
        }

        private async Task<string> InstallForgeAsync(MinecraftLauncher launcher, string mcVersion, string fVersion, IProgress<double> progress = null)
        {
            try
            {
                var fInstaller = new ForgeInstaller(launcher);
                var installOptions = new ForgeInstallOptions
                {
                    ByteProgress = new Progress<ByteProgress>(e => { Console.WriteLine(e); progress?.Report(e.ToRatio() * 100); }),
                    InstallerOutput = new Progress<string>(e =>
                        Console.WriteLine(e)),
                    CancellationToken = CancellationToken.None,
                    SkipIfAlreadyInstalled = true,
                };

                var latestForgeInstallName = await fInstaller.Install(mcVersion, fVersion, installOptions);
                await launcher.InstallAsync(latestForgeInstallName);
                return latestForgeInstallName;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("An error occured during forge installation.");
                return null;
            }
        }

        private MinecraftPath CreateProfilePath(string profileName)
        {
            string profilePath = Path.Combine(Constants.ProfilesPath, profileName);
            if (!Directory.Exists(profilePath))
            {
                Directory.CreateDirectory(profilePath);
            }

            MinecraftPath path = new MinecraftPath(profilePath);
            path.Library = Path.Combine(Constants.SharedPath, "commons", "libraries");
            path.Versions = Path.Combine(Constants.SharedPath, "commons", "versions");
            path.Assets = Path.Combine(Constants.SharedPath, "assets");
            path.Runtime = Path.Combine(Constants.SharedPath, "java");

            return path;
        }
    }
}
