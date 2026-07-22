using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TCM_Launcher.Interfaces;
using TCM_Launcher.MVVM;
using TCML_Class_library;
using static TCM_Launcher.ViewModel.MainWindowViewModel;

namespace TCM_Launcher.ViewModel.UserControls
{
    public class AddContentViewModel : ViewModelBase
    {
        private IBackendService backendService;
        public AddContentViewModel(IBackendService backendService)
        {
            this.backendService = backendService;
        }

        public Func<ContentToShow, Task>? OnCloseRequested { get; set; }
        public Func<string, ModSource, Task> OnModDetailsRequested { get; set; }

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

            }
        }

        private async Task OpenModDetailsAsync()
        {
            if (SelectedSearchResult?.Mod == null) return;
            if (OnModDetailsRequested != null)
            {
                await OnModDetailsRequested.Invoke(SelectedSearchResult.Mod.Id, SelectedSearchResult.Mod.Source);
            }
        }

        public void Close()
        {
            SearchText = "";
            OnCloseRequested.Invoke(ContentToShow.ProfileDetails);
        }

	}
}
    