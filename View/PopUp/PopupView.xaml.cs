using System.Windows;
using TCM_Launcher.ViewModel.UI.Popup;

namespace TCM_Launcher.View.PopUp
{
    public partial class PopupView : Window
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="labelText">Text to be displayed in the windwos label.</param>
        /// <param name="text">Text to be displayed in the windwos textblock.</param>
        /// <param name="remainTime">Time till the window is open. After this it closes.</param>
        /// <param name="profileId">Id of the profile.</param>
        public PopupView(string labelText, string text, PopupAction type, double? remainTime = null, string? profileId = null)
        {
            InitializeComponent();
            viewModel = new PopupViewModel(this, labelText, text, type, remainTime, profileId);
            DataContext = viewModel;
        }

        PopupViewModel viewModel;

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
