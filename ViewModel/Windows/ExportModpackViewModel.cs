using System.Collections.ObjectModel;
using System.Windows.Data;
using TCM_Launcher.Interfaces;
using TCM_Launcher.MVVM;
using TCM_Launcher.ViewModel.UserControls.ControlItems;

namespace TCM_Launcher.ViewModel.Windows
{
    public class ExportModpackViewModel : ViewModelBase
    {
        private readonly IProfileModService profileModService;
        public string ProfileId { private get; set; }
        public string ProfileName { private get; set; }
        public ExportModpackViewModel(IProfileModService profileModService)
        {
            this.profileModService = profileModService;
        }

        private bool exportJARFiles = true;
        public bool ExportJARFiles
        {
            get { return exportJARFiles; }
            set 
            { 
                exportJARFiles = value; 
                OnPropertyChange();
                if (value == false) ExportOnlyImportedFiles = false;
            }
        }

        private bool exportOnlyImportedFiles;
        public bool ExportOnlyImportedFiles
        {
            get { return exportOnlyImportedFiles; }
            set { exportOnlyImportedFiles = value; OnPropertyChange(); }
        }


        private ObservableCollection<ProfileModViewModel> exportableMods;
		public ObservableCollection<ProfileModViewModel> ExportableMods
		{
			get { return exportableMods; }
			set { exportableMods = value; OnPropertyChange(); }
		}

        public void ToggleModExport(string modId, bool isChecked)
        {
            var existing = ExportableMods.FirstOrDefault(m => m.Mod.Id == modId);
            if(existing != null)
            {
                existing.Mod.IsEnabled = isChecked;
                OnPropertyChange(nameof(existing.Mod.IsEnabled));
            }
        }

        public void ToggleAllModsExport(bool isChecked)
        {
            foreach (var model in ExportableMods)
            {
                if(model.Mod.IsEnabled != isChecked) model.Mod.IsEnabled = isChecked;
            }
            CollectionViewSource.GetDefaultView(ExportableMods)?.Refresh();
        }

        public async Task ExportModpack()
        {
            await profileModService.ExportModpackAsync(ProfileId, ProfileName, ExportJARFiles, ExportableMods.Select(vm => vm.Mod).ToList(), ExportOnlyImportedFiles);
        }
    }
}
