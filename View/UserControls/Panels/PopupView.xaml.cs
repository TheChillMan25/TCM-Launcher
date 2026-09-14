using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.ViewModel.UserControls.Panels;

namespace TCM_Launcher.View.UserControls.Panels
{
    public partial class PopupView : UserControl
    {
        private PopupViewModel viewModel => DataContext as PopupViewModel;
        public PopupView()
        {
            InitializeComponent();
        }

        public void Initialize(string labelText, string text, PopupAction type, double? remainTime = null, string? profileId = null)
        {
            viewModel.Initialize(labelText, text, type, remainTime, profileId);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ManageActions();
        }
    }
}
