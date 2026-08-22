using System.Windows;
using TCM_Launcher.ViewModel.Popup;

namespace TCM_Launcher.View.PopUp
{
    public partial class PopupView : Window
    {
        private readonly PopupViewModel viewModel;
        public PopupView(PopupViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }

        public void Initialize(string labelText, string text, PopupAction type, double? remainTime = null, string? profileId = null)
        {
            viewModel.Initialize(this, labelText, text, type, remainTime, profileId);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ManageActions();
        }
    }
}
