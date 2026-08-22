using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.ProcessBuilder;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.View.PopUp;
using TCM_Launcher.ViewModel.Popup;
using static TCM_Launcher.Core.Utils.Constants;

namespace TCM_Launcher.Services
{
    public class LauncherService : ILauncherService
    {
        private readonly IProfileSettingsService profileSettingsService;
        private readonly IMicrosoftService microsoftService;
        private readonly IGameProfileService gameProfileService;
        private readonly IProfileModService profileModService;
        private readonly IAppSettingsService appSettingsService;

        public LauncherService(IProfileSettingsService profileSettingsService, 
            IMicrosoftService microsoftService, 
            IGameProfileService gameProfileService, 
            IProfileModService profileModService,
            IAppSettingsService appSettingsService)
        {
            this.profileSettingsService = profileSettingsService;
            this.gameProfileService = gameProfileService;
            this.microsoftService = microsoftService;
            this.profileModService = profileModService;
            this.appSettingsService = appSettingsService;
        }

        public async Task<string> CreateProfileAsync(string pId, string mcVersion, string fVersion, IProgress<double>? progress = null, IProgress<string>? status = null, IProgress<bool>? progressVisible = null)
        {
            try
            {
                progressVisible?.Report(true);
                MinecraftPath path = CreateProfilePath(pId);
                var launcher = new MinecraftLauncher(path);
                launcher.ByteProgressChanged += (sender, args) =>
                {
                    if (args.TotalBytes > 0)
                    {
                        double percentage = (double)args.ProgressedBytes / args.TotalBytes * 100;
                        progress?.Report(percentage);
                    }
                };
                return await InstallForgeAsync(launcher, mcVersion, fVersion, progress, status, progressVisible);
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception during creating profile with data: ProfileName={pId}, MCVersion={mcVersion}, ForgeVersion={fVersion}", ex);
                throw;
            }
        }

        public async Task LaunchProfileAsync(GameProfile profile, IProgress<double>? progress = null, IProgress<string>? status = null, IProgress<bool>? progressVisible = null, string? serverAddress = null)
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
                progressVisible?.Report(true);
                status?.Report("Starting game");
                progress?.Report(10);
                var profileSettings = await profileSettingsService.GetProfileSettings(profile.Id);
                progress?.Report(30);
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
                MinecraftPath path = CreateProfilePath(profile.Id);
                var launcher = new MinecraftLauncher(path);
                launcher.ByteProgressChanged += (sender, args) =>
                {
                    if (args.TotalBytes > 0)
                    {
                        double percent = (double)args.ProgressedBytes / args.TotalBytes * 100;
                        progress?.Report(percent);
                    }
                };
                status?.Report("Checking file integrity");
                await launcher.InstallAsync(profile.FileName);
                var launchOptions = new MLaunchOption
                {
                    MaximumRamMb = profileSettings.Ram ?? Constants.DefaultRam,
                    Session = microsoftService.MSession == null ? MSession.CreateOfflineSession("Gamer123") : microsoftService.MSession,
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
                bool syncSuccess = await profileModService.SyncProfileModsAsync(profile.Id, progress, status, serverAddress);
                if (!syncSuccess)
                {
                    progressVisible?.Report(false);
                    return;
                }
                status?.Report("Starting game");
                var process = await launcher.BuildProcessAsync(profile.FileName, launchOptions);
                var processWrapper = new ProcessWrapper(process);
                processWrapper.OutputReceived += (sender, log) =>
                {
                    if (!string.IsNullOrWhiteSpace(log))
                    {
                        Logger.Log(log, "MINECRAFT");
                    }
                };
                
                await gameProfileService.UpdateLastPlayedProfileAsync(profile.Id);
                Logger.Log($"Started game profile with id: {profile.Id} ({profile.FileName})");
                progress?.Report(100);
                progressVisible?.Report(false);

                if (appSettingsService.AppSettings.OnGameStart == LauncherWindowBehaviour.Minimize)
                    appSettingsService.LauncherWindowBehaviour(appSettingsService.AppSettings.OnGameStart, true);

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
                    var p = App.ServiceProvider.GetRequiredService<PopupView>();
                    p.Initialize($"The game crashed (Code: {exitCode})", crashReportInfo, PopupAction.CRASH, 5000d, profile.Id);
                    p.Owner = Application.Current.MainWindow;
                    p.Show();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an exception during launching profile with id: {profile.Id}", ex);
                MessageBox.Show("An error occured during launching game.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (appSettingsService.AppSettings.OnGameStart == LauncherWindowBehaviour.Minimize)
                    appSettingsService.LauncherWindowBehaviour(appSettingsService.AppSettings.OnGameStart, false);
            }
            
        }

        private async Task<string> InstallForgeAsync(MinecraftLauncher launcher, string mcVersion, string fVersion, IProgress<double>? progress = null, IProgress<string>? status = null, IProgress<bool>? progressVisible = null)
        {
            try
            {
                status?.Report("Installing forge files");
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
                status?.Report("Installing minecraft files");
                await launcher.InstallAsync(latestForgeInstallName);
                Logger.Log($"Installing forge ({fVersion} for minecraft with version {mcVersion}) completed", "FORGE");
                progressVisible?.Report(false);
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
