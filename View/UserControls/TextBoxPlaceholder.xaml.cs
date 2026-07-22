using System.Windows;
using System.Windows.Controls;

namespace TCM_Launcher.View.UserControls
{
    public partial class TextBoxPlaceholder : UserControl
    {
        public TextBoxPlaceholder()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                nameof(Placeholder),
                typeof(string),
                typeof(TextBoxPlaceholder),
                new PropertyMetadata(string.Empty));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            DependencyProperty.Register(
                nameof(Input),
                typeof(string),
                typeof(TextBoxPlaceholder),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Input
        {
            get { return (string)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Input))
            {
                PlaceholderBox.Visibility = Visibility.Hidden;
            }
            else
            {
                PlaceholderBox.Visibility = Visibility.Visible;
            }
        }
    }
}
