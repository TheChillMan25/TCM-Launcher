using System.Windows;
using System.Windows.Controls;
using TCM_Launcher.Model.DB;
using TCM_Launcher.ViewModel.UI.UserControls;

namespace TCM_Launcher.View.UserControls
{
    public partial class ServerCardView : UserControl
    {
        public ServerCardViewModel viewModel;
        public ServerCardView()
        {
            InitializeComponent();
            viewModel = new ServerCardViewModel();
            RootGrid.DataContext = viewModel;
        }

        public static readonly RoutedEvent DeleteRequestedEvent = EventManager.RegisterRoutedEvent(
            nameof(DeleteRequested),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ServerCardView));

        public event RoutedEventHandler DeleteRequested
        {
            add { AddHandler(DeleteRequestedEvent, value); }
            remove { RemoveHandler(DeleteRequestedEvent, value); }
        }

        public static readonly RoutedEvent UpdatedServerEvent = EventManager.RegisterRoutedEvent(
            nameof(UpdatedServer),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ServerCardView));

        public event RoutedEventHandler UpdatedServer
        {
            add { AddHandler(UpdatedServerEvent, value); }
            remove { RemoveHandler(UpdatedServerEvent, value); }
        }

        public static readonly RoutedEvent QuickLaunchEvent = EventManager.RegisterRoutedEvent(
            nameof(QuickLaunch),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ServerCardView));

        public event RoutedEventHandler QuickLaunch
        {
            add { AddHandler(QuickLaunchEvent, value); }
            remove { RemoveHandler(QuickLaunchEvent, value); }
        }

        public static readonly DependencyProperty ServerProperty =
            DependencyProperty.Register(
                nameof(Server),
                typeof(Server),
                typeof(ServerCardView),
                new PropertyMetadata(null, OnServerChange));

        private Server server;
        public Server Server
        {
            get { return (Server)GetValue(ServerProperty); }
            set { SetValue(ServerProperty, value); }
        }
        private static void OnServerChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ServerCardView view)
            {
                var server = e.NewValue as Server;

                if (view.viewModel != null && server != null)
                {
                    view.viewModel.Server = server;
                }
            }
        }

        private async void QuickStartButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(QuickLaunchEvent));
            QuickStartButton.IsEnabled = false;
            await viewModel.QuickLaunchAsync();
            QuickStartButton.IsEnabled = true;
            RaiseEvent(new RoutedEventArgs(QuickLaunchEvent));
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.VerticalOffset = 5;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            bool edit = await viewModel.EditServer();
            if (edit) RaiseEvent(new RoutedEventArgs(UpdatedServerEvent));
        }

        private async void Remove_Click(object sender, RoutedEventArgs e)
        {
            bool success = await viewModel.DeleteServer();
            if (success) RaiseEvent(new RoutedEventArgs(DeleteRequestedEvent));
        }
    }
}
