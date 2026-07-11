using System.IO;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.Services;
using TCM_Launcher.View.Windows;

namespace TCM_Launcher.ViewModel.UI.UserControls
{
    internal class ProfileDetailsViewModel : ViewModelBase
    {
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


        public void OpenProfileSettings()
        {
            if (SelectedGameProfile == null)
            {
                MessageBox.Show("Select a profile.");
                return;
            }
            ProfileSettingsView ps = new ProfileSettingsView(SelectedGameProfile);
            ps.Owner = Application.Current.MainWindow;
            Application.Current.MainWindow.Opacity = 0.4;
            ps.ShowDialog();
            Application.Current.MainWindow.Opacity = 1;
        }

        public bool DeleteProfile()
        {
            try
            {
                bool success = GameProfileService.Instance.DeleteProfile(SelectedGameProfile.Id);
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
                MessageBox.Show("Couldn't delete the profile because it is still running or the files are still in use. Close the game before deleting.", "Deletion error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception during deleting profile", ex);
                MessageBox.Show("An error occured during deleting profile.");
                return false;
            }

        }

        public void OpenProfileFolder()
        {
            GameProfileService.Instance.OpenProfileFolder(SelectedGameProfile.Id);
        }

        public async Task StartGame()
        {
            try
            {
                SelectedGameProfile.IsPlaying = true;
                IsEnable = SelectedGameProfile.Installed && !SelectedGameProfile.IsPlaying;
                PlayButtonText = "Running";
                await LauncherService.Instance.LaunchProfileAsync(SelectedGameProfile.Id, SelectedGameProfile.FileName!);
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
