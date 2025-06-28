using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cleo.Controls
{
    public partial class MessageBubble : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MessageBubble),
                new PropertyMetadata(string.Empty, OnMessageChanged));

        public static readonly DependencyProperty IsUserProperty =
            DependencyProperty.Register("IsUser", typeof(bool), typeof(MessageBubble),
                new PropertyMetadata(false, OnIsUserChanged));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public bool IsUser
        {
            get => (bool)GetValue(IsUserProperty);
            set => SetValue(IsUserProperty, value);
        }

        public MessageBubble()
        {
            InitializeComponent();
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageBubble bubble)
            {
                bubble.MessageText.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private static void OnIsUserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageBubble bubble && e.NewValue is bool isUser)
            {
                if (isUser)
                {
                    // User message styling
                    bubble.BubbleBorder.Background = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["PrimaryColor"];
                    bubble.MessageText.Foreground = System.Windows.Media.Brushes.White;
                    bubble.BubbleBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                }
                else
                {
                    // AI message styling
                    bubble.BubbleBorder.Background = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["SurfaceColor"];
                    bubble.MessageText.Foreground = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["TextColor"];
                    bubble.BubbleBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                }
            }
        }
    }
}
