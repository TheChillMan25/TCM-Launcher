using System.Windows;
using System.Windows.Controls;

namespace TCM_Launcher.View.UserControls.Misc
{
    public partial class CustomTooltip : UserControl
    {
        public CustomTooltip()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TooltipLabelTextProperty =
            DependencyProperty.Register(
                nameof(TooltipLabelText),
                typeof(string),
                typeof(CustomTooltip),
                new PropertyMetadata("", OnTooltipLabelTextChanged));

        public string TooltipLabelText
        { 
            get { return (string)GetValue(TooltipLabelTextProperty); }
            set { SetValue(TooltipLabelTextProperty, value); } 
        }

        private static void OnTooltipLabelTextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CustomTooltip t)
            {
                t.TooltipLabel.Content = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        public static readonly DependencyProperty TooltipContentTextProperty =
            DependencyProperty.Register(
                nameof(TooltipContentText),
                typeof(string),
                typeof(CustomTooltip),
                new PropertyMetadata("", OnTooltipContentTextChanged));

        public string TooltipContentText
        {
            get { return (string)GetValue(TooltipContentTextProperty); }
            set { SetValue(TooltipContentTextProperty, value); }
        }

        private static void OnTooltipContentTextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CustomTooltip t)
            {
                t.TooltipTextBox.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }
    }
}
