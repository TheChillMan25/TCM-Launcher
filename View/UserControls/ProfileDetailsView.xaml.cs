using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.Model.DB;
using TCM_Launcher.ViewModel.UserControls;

namespace TCM_Launcher.View.UserControls
{
    public partial class ProfileDetailsView : UserControl
    {
        private readonly ProfileDetailsViewModel viewModel;
        public ProfileDetailsView()
        {
            InitializeComponent();

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                this.viewModel = App.ServiceProvider.GetRequiredService<ProfileDetailsViewModel>();
                RootGrid.DataContext = this.viewModel;
            }
        }

        public static readonly RoutedEvent DeleteRequestedEvent = EventManager.RegisterRoutedEvent(
            nameof(DeleteRequested),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ProfileDetailsView));

        public event RoutedEventHandler DeleteRequested
        {
            add { AddHandler(DeleteRequestedEvent, value); }
            remove { RemoveHandler(DeleteRequestedEvent, value); }
        }

        public static DependencyProperty SelectedGameProfileProperty =
            DependencyProperty.Register(
                nameof(SelectedGameProfile),
                typeof(GameProfile),
                typeof(ProfileDetailsView),
                new PropertyMetadata(null, OnProfileChanged));

        public GameProfile SelectedGameProfile 
        { 
            get { return (GameProfile)GetValue(SelectedGameProfileProperty); }
            set { SetValue(SelectedGameProfileProperty, value); }
        }

        private static void OnProfileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is ProfileDetailsView view)
            {
                var newProfile = e.NewValue as GameProfile;

                if(view.viewModel != null && newProfile != null)
                {
                    view.viewModel.SelectedGameProfile = newProfile;
                    view.viewModel.IsEnable = (newProfile.Installed == true && !newProfile.IsPlaying);
                    view.viewModel.PlayButtonText = newProfile.IsPlaying ? "Running" : "Play";
                }
            }
        }

        public static DependencyProperty DownloadProgressProperty =
            DependencyProperty.Register(
                nameof(DownloadProgress),
                typeof(double),
                typeof(ProfileDetailsView),
                new PropertyMetadata(0d, OnIsDownloadingChange));

        public double DownloadProgress
        {
            get { return (double)GetValue(DownloadProgressProperty); }
            set { SetValue(DownloadProgressProperty, value); }
        }

        private static void OnIsDownloadingChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProfileDetailsView view)
            {
                if(e.NewValue is double newValue)
                {
                    if (view.viewModel != null) view.viewModel.DownloadProgress = newValue;
                }
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //var p = new PopupView("The game crashed (exitcode: 1)", "Further details of the crash report here: ....", 5000, "6dfb1b9b-3bda-48fc-81bb-ec6810f140b6");
            //p.Owner = this;
            //p.Show();
            if (viewModel.SelectedGameProfile == null)
            {
                MessageBox.Show("Select a profile to START.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (viewModel.SelectedGameProfile.Installed != true)
            {
                MessageBox.Show("Profile is installing files. Please wait", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await viewModel.StartGame();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OpenProfileSettings();
        }

        private void OtherButton_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.VerticalOffset = 5;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OpenProfileFolder();
        }

        private async void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            bool success = await viewModel.DeleteProfileAsync();
            if(success) RaiseEvent(new RoutedEventArgs(DeleteRequestedEvent));
        }

        //private void ExportModpack_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Modpack exportalasa...");
        //}
    }
}
