using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.ProcessBuilder;
using System.IO;
using System.Net;
using System.Windows;
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

        public async Task LaunchProfileAsync(GameProfile profile, string? serverAddress = null)
        {
            try
            {
                bool isInternet = await NetworkUtil.IsInternetAvailableAsync();
                if (!isInternet)
                {
                    Logger.Error("No internet. Connect to the internet before launching game.");
                    MessageBox.Show(" " +
                        "No internet. Connect to the internet before launching game.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MinecraftPath path = CreateProfilePath(profile.Id);
                var launcher = new MinecraftLauncher(path);
                var profileSettings = await ProfileSettingsService.Instance.GetProfileSettings(profile.Id);
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
                await launcher.InstallAsync(profile.FileName);
                var launchOptions = new MLaunchOption
                {
                    MaximumRamMb = profileSettings.Ram ?? Constants.DefaultRam,
                    Session = MicrosoftService.Instance.MSession == null ? MSession.CreateOfflineSession("Gamer123") : MicrosoftService.Instance.MSession,
                    ExtraJvmArguments = jvmArgs,
                };
                if(serverAddress != null)
                {
                    string ip = serverAddress;
                    ushort port = 25565;

                    if (ip.Contains(":"))
                    {
                        var parts = ip.Split(":");
                        ip = parts[0];
                        ushort.TryParse(parts[1], out port);
                    }

                    launchOptions.ServerIp = ip;
                    launchOptions.ServerPort = port;
                }
                var process = await launcher.BuildProcessAsync(profile.FileName, launchOptions);
                var processWrapper = new ProcessWrapper(process);
                processWrapper.OutputReceived += (sender, log) =>
                {
                    if (!string.IsNullOrWhiteSpace(log))
                    {
                        Logger.Log(log, "MINECRAFT");
                    }
                };
                
                GameProfileService.Instance.UpdateLastPlayedProfileAsync(profile.Id);
                Logger.Log($"Started game profile with id: {profile.Id} ({profile.FileName})");
                processWrapper.StartWithEvents();

                int exitCode = await processWrapper.WaitForExitTaskAsync();
                if (exitCode == 0)
                {
                    Logger.Log($"Game with id {profile.Id} terminated successfully");
                }
                else
                {
                    string crashReportFolder = Path.Combine(Constants.ProfilesPath, profile.Id,"crash-reports");
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
                    Logger.Log($"Game with id {profile.Id} crashed. {crashReportInfo}");
                    var p = new PopupView($"The game crashed (Code: {exitCode})", crashReportInfo, PopupAction.CRASH, 5000d, profile.Id);
                    p.Owner = Application.Current.MainWindow;
                    p.Show();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception during launching profile with id: {profile.Id}", ex);
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
