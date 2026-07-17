using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.View.Windows;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class ProfileDetailsViewModel : ViewModelBase
    {
        private readonly IGameProfileService gameProfileService;
        private readonly ILauncherService launcherService;
        public ProfileDetailsViewModel(IGameProfileService gameProfileService, ILauncherService launcherService )
        {
            this.gameProfileService = gameProfileService;
            this.launcherService = launcherService;
        }

        private GameProfile selectedGameProfile;

        public GameProfile SelectedGameProfile
        {
            get { return selectedGameProfile; }
            set
            {
                selectedGameProfile = value;
                OnPropertyChange();
                if(selectedGameProfile != null) HeaderText = $"Forge {selectedGameProfile?.MCVersion}";
            }
        }

        private string headerText;

        public string HeaderText
        {
            get { return headerText; }
            set
            {
                headerText = value;
                OnPropertyChange();
            }
        }

        private double downloadProgress;
        public double DownloadProgress
        {
            get { return downloadProgress; }
            set
            {
                downloadProgress = value;
                OnPropertyChange();
            }
        }

        private bool isEnable;

        public bool IsEnable
        {
            get { return isEnable; }
            set 
            { 
                isEnable = value;
                OnPropertyChange();
            }
        }

        private string playButtonText = "Play";

        public string PlayButtonText
        {
            get { return playButtonText; }
            set 
            { 
                playButtonText = value;
                OnPropertyChange();
            }
        }


        public async Task OpenProfileSettings()
        {
            if (SelectedGameProfile == null)
            {
                MessageBox.Show("Select a profile.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var ps = App.ServiceProvider.GetRequiredService<ProfileSettingsView>();
            await ps.Initialize(SelectedGameProfile);
            ps.Owner = Application.Current.MainWindow;
            Application.Current.MainWindow.Opacity = 0.4;
            ps.ShowDialog();
            Application.Current.MainWindow.Opacity = 1;
        }

        public async Task<bool> DeleteProfileAsync()
        {
            try
            {
                bool success = await gameProfileService.DeleteProfileAsync(SelectedGameProfile.Id);
                if (success)
                {
                    string profileDir = Path.Combine(Constants.ProfilesPath, SelectedGameProfile.Id);
                    if (Directory.Exists(profileDir)) Directory.Delete(profileDir, true);
                }
                return success;
            }
            catch (IOException ex)
            {
                Logger.Error("There was an exception during deleting profile", ex);
                MessageBox.Show("Couldn't delete the profile because it is still running or the files are still in use. Close the game before deleting.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception during deleting profile", ex);
                MessageBox.Show("An error occured during deleting profile.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

        }

        public void OpenProfileFolder()
        {
            gameProfileService.OpenProfileFolder(SelectedGameProfile.Id);
        }

        public async Task StartGame()
        {
            try
            {
                SelectedGameProfile.IsPlaying = true;
                IsEnable = SelectedGameProfile.Installed && !SelectedGameProfile.IsPlaying;
                PlayButtonText = "Running";
                await launcherService.LaunchProfileAsync(SelectedGameProfile);
            }
            finally
            {
                SelectedGameProfile.IsPlaying = false;
                PlayButtonText = "Play";
                IsEnable = SelectedGameProfile.Installed && !SelectedGameProfile.IsPlaying;
            }
        }
    }
}
