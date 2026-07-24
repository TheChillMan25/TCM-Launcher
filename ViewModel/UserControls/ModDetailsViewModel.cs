using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using TCM_Launcher.Interfaces;
using TCM_Launcher.MVVM;
using TCML_Class_library;
using static TCM_Launcher.ViewModel.MainWindowViewModel;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class ModDetailsViewModel : ViewModelBase
    {

		private IDownloadService downloadService;
        private IProfileModService profileModService;
        public ModDetailsViewModel(IDownloadService downloadService, IProfileModService profileModService)
        {
            this.downloadService = downloadService;
			this.profileModService = profileModService;
        }

		public Func<ContentToShow, Task>? OnBackToBrowseRequested;

        public string ProfileId { get; set; }
        public string MCVersion { get; set; }

        private ModDetails details;
		public ModDetails Details
		{
			get { return details; }
			set 
			{ 
				details = value;
				OnPropertyChange();
			}
		}

		private ObservableCollection<ModVersionCardViewModel> versions;
		public ObservableCollection<ModVersionCardViewModel> Versions
		{
			get { return versions; }
			set { versions = value; OnPropertyChange(); }
		}

		private bool installEnabled = true;
		public bool IsLatestBindedToProfile
        {
			get { return installEnabled; }
			set { installEnabled = value; OnPropertyChange(); }
		}

		private string installText;
		public string InstallText
		{
			get { return installText; }
			set { installText = value; OnPropertyChange(); }
		}

		public async Task SetVersionsAsync()
		{
            if (Details?.Versions == null) return;
            var versions = Details.Versions.Select(v =>
			{
				var mVVM = App.ServiceProvider.GetRequiredService<ModVersionCardViewModel>();
				mVVM.Version = v;
				mVVM.ProfileId = ProfileId;
				mVVM.OnAddToProfileRequested = AddModToProfileAsync;
				return mVVM;
			}).ToList();
			Versions = new ObservableCollection<ModVersionCardViewModel>(versions);
			await SetLatestBindedEnabled();
            await RefreshVersionCardsStatusAsync();
		}

        private async Task RefreshVersionCardsStatusAsync()
        {
            if (Versions == null || !Versions.Any()) return;
            var tasks = Versions.Select(v => v.SetIsBindedToProfile());
            await Task.WhenAll(tasks);
        }

        public async Task AddLatestAsync()
		{
			var latest = Versions.FirstOrDefault()?.Version;
			if (latest != null) await AddModToProfileAsync(latest);
			else MessageBox.Show("There was an error fetching file data.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			await SetLatestBindedEnabled(latest);
		}

		public async Task AddModToProfileAsync(ModVersion version)
        {
			await profileModService.AddModWithDependenciesAsync(ProfileId, MCVersion, Details, version);
			await RefreshVersionCardsStatusAsync();
			await SetLatestBindedEnabled();
        }

        private async Task SetLatestBindedEnabled(ModVersion? latest = null)
        {
			if(latest == null) latest = Versions.FirstOrDefault()?.Version;
            IsLatestBindedToProfile = await profileModService.IsVersionBindedToProfileAsync(ProfileId, latest);
			InstallText = IsLatestBindedToProfile ? "Installed" : "Install";
        }

        public void BackToBrowse()
        {
			OnBackToBrowseRequested.Invoke(ContentToShow.AddContent);
        }
    }
}
