using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model.DB;
using TCM_Launcher.MVVM;
using TCM_Launcher.ViewModel.UserControls.ControlItems;
using TCML_Class_library;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class AddContentViewModel : ViewModelBase
    {
        private IBackendService backendService;
        public AddContentViewModel(IBackendService backendService)
        {
            this.backendService = backendService;
        }

        public Func<GameProfile, Task>? OnCloseRequested { get; set; }
        public Func<string, ModSearchResult, string, Task> OnModDetailsRequested { get; set; }

        private string searchText;
		public string SearchText
		{
			get { return searchText; }
			set 
			{ 
				searchText = value;
				OnPropertyChange();
                _ = Search();
            }
		}

        private CancellationTokenSource _searchCts;

		private string profileName;
		public string ProfileName
		{
			get { return profileName; }
			set { profileName = value; OnPropertyChange(); }
		}
		private string profileId;
		public string ProfileId
		{
			get { return profileId; }
			set { profileId = value; OnPropertyChange(); }
		}

		private string mcVersion;
		public string MCVersion
		{
			get { return mcVersion; }
			set { mcVersion = value; OnPropertyChange(); }
		}

        private ObservableCollection<ModSearchResultCardViewModel> searchResult = new ObservableCollection<ModSearchResultCardViewModel>();
        public ObservableCollection<ModSearchResultCardViewModel> SearchResult
        {
            get { return searchResult; }
            set { searchResult = value; OnPropertyChange(); }
        }

        private ModSearchResultCardViewModel selectedSearchResult;

        public ModSearchResultCardViewModel SelectedSearchResult
        {
            get { return selectedSearchResult; }
            set 
            { 
                selectedSearchResult = value;
                OnPropertyChange();
                if (selectedSearchResult != null)
                {
                    _ = OpenModDetailsAsync();
                }
            }
        }

        private async Task Search()
		{
            _searchCts?.Cancel();

            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(500, token);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var result = await backendService.SearchModsAsync(SearchText, MCVersion, token);
                    var resVMs = result.Select(m =>
                    {
                        var cardVM = App.ServiceProvider.GetRequiredService<ModSearchResultCardViewModel>();
                        cardVM.Mod = m;
                        return cardVM;
                    });
                    SearchResult = new ObservableCollection<ModSearchResultCardViewModel>(resVMs);
                }
                else
                {
                    SearchResult = new ObservableCollection<ModSearchResultCardViewModel>();
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                Logger.Error("There was an exception when searching for content.", ex);
            }
        }

        private async Task OpenModDetailsAsync()
        {
            if (SelectedSearchResult?.Mod == null) return;
            if (OnModDetailsRequested != null)
            {
                await OnModDetailsRequested.Invoke(ProfileId, SelectedSearchResult.Mod, MCVersion);
            }
        }

        public void Close()
        {
            SearchText = "";
            OnCloseRequested?.Invoke(null);
        }

	}
}
    