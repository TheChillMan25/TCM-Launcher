using System.Windows;
using System.Windows.Controls;

namespace TCM_Launcher.View.UserControls
{
    public partial class TextBoxWithPlaceholder : UserControl
    {
        public TextBoxWithPlaceholder()
        {
            InitializeComponent();
        }

        private string text;

        public string Text
        {
            get { return Input.Text; }
            set { Input.Text = value; }
        }


        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                nameof(Placeholder),
                typeof(string),
                typeof(TextBoxWithPlaceholder),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBoxWithPlaceholder control)
            {
                control.PlaceholderText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }


        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(Input.Text)) PlaceholderText.Visibility = Visibility.Visible;
            else PlaceholderText.Visibility = Visibility.Hidden;
        }
    }
}
