using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.ProcessBuilder;
using System.IO;
using System.Net.Http;
using System.Windows;
using TCM_Launcher.Core;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Model.DB;
using TCM_Launcher.View.PopUp;
using TCM_Launcher.ViewModel.UI.Popup;

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
                };
                return await InstallForgeAsync(launcher, mcVersion, fVersion, progress);
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception during creating profile with data: ProfileName={pName}, MCVersion={mcVersion}, ForgeVersion={fVersion}", ex);
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
                    if (!string.IsNullOrWhiteSpace(log))
                    {
                        Logger.Log(log, "MINECRAFT");
                    }
                };
                
                GameProfileService.Instance.UpdateLastPlayedProfile(profileId);
                Logger.Log($"Started game profile with id: {profileId} ({fileName})");
                processWrapper.StartWithEvents();

                int exitCode = await processWrapper.WaitForExitTaskAsync();
                if (exitCode == 0)
                {
                    Logger.Log($"Game with id {profileId} terminated successfully");
                }
                else
                {
                    string crashReportFolder = Path.Combine(Constants.ProfilesPath, profileId,"crash-reports");
                    string crashReportInfo = "";

                    if (Directory.Exists(crashReportFolder))
                    {
                        var latestCrash = new DirectoryInfo(crashReportFolder)
                            .GetFiles("crash-*.txt")
                            .OrderByDescending(f => f.CreationTime)
                            .FirstOrDefault();

                        if (latestCrash != null)
                        {
                            crashReportInfo = $"An overview of the crash report saved here: {latestCrash}";
                        }
                    }
                    Logger.Log($"Game with id {profileId} crashed. {crashReportInfo}");
                    var p = new PopupView($"The game crashed (Code: {exitCode})", crashReportInfo, PopupAction.CRASH, 5000d, profileId);
                    p.Owner = Application.Current.MainWindow;
                    p.Show();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception during launching profile with id: {profileId}", ex);
                MessageBox.Show("An error occured during launching game.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private async Task<string> InstallForgeAsync(MinecraftLauncher launcher, string mcVersion, string fVersion, IProgress<double> progress = null)
        {
            try
            {
                Logger.Log($"Installing forge ({fVersion} for minecraft with version {mcVersion}) started", "FORGE");
                var fInstaller = new ForgeInstaller(launcher);
                var installOptions = new ForgeInstallOptions
                {
                    ByteProgress = new Progress<ByteProgress>(e => progress?.Report(e.ToRatio() * 100)),
                    InstallerOutput = new Progress<string>(e =>
                    {
                        if (!string.IsNullOrWhiteSpace(e))
                        {
                            Logger.Log(e, "FORGE");
                        }
                    }),
                    CancellationToken = CancellationToken.None,
                    SkipIfAlreadyInstalled = true,
                };

                var latestForgeInstallName = await fInstaller.Install(mcVersion, fVersion, installOptions);
                await launcher.InstallAsync(latestForgeInstallName);
                Logger.Log($"Installing forge ({fVersion} for minecraft with version {mcVersion}) completed", "FORGE");
                return latestForgeInstallName;
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception during installing forge\nData: MCVersion={mcVersion}, ForgeVersion={fVersion}", ex);
                MessageBox.Show("An error occured during forge installation.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
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
