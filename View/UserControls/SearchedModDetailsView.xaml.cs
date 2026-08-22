using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TCM_Launcher.ViewModel.UserControls;

namespace TCM_Launcher.View.UserControls
{
    public partial class SearchedModDetailsView : UserControl
    {
        private SearchedModDetailsViewModel viewModel => DataContext as SearchedModDetailsViewModel;
        public SearchedModDetailsView()
        {
            InitializeComponent();
        }

        private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string url && Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url)
                    {
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Nem sikerült megnyitni a linket: {ex.Message}");
                }
            }
        }

        private void MarkdownViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(sender as DependencyObject);

            if (scrollViewer != null)
            {
                e.Handled = true;

                double scrollMultiplier = 1.5;

                double newOffset = scrollViewer.VerticalOffset - (e.Delta * scrollMultiplier / 6);
                scrollViewer.ScrollToVerticalOffset(newOffset);
            }
        }

        private T? FindVisualChild<T>(DependencyObject? obj) where T : DependencyObject
        {
            if (obj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t) return t;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }

        private async void InstallLatestButton_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.AddLatestAsync();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.BackToBrowse();
        }
    }
}
