using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Speech.Recognition;
using Cleo.Services;
using Cleo.Controls;

namespace Cleo
{
    public partial class MainWindow : Window
    {
        private readonly AIService _aiService;
        private readonly SpeechRecognitionEngine _speechRecognizer;
        private readonly DispatcherTimer _resetTimer;
        private bool _isListening = false;

        public MainWindow()
        {
            InitializeComponent();
            _aiService = new AIService();
            _speechRecognizer = InitializeSpeechRecognition();

            // Initialize chat reset timer (5 second delay)
            _resetTimer = new DispatcherTimer();
            _resetTimer.Interval = TimeSpan.FromSeconds(5);
            _resetTimer.Tick += OnResetTimerTick;
        }

        private SpeechRecognitionEngine InitializeSpeechRecognition()
        {
            var recognizer = new SpeechRecognitionEngine();
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.LoadGrammar(new DictationGrammar());
            recognizer.SpeechRecognized += OnSpeechRecognized;
            recognizer.RecognizeCompleted += OnRecognizeCompleted;
            return recognizer;
        }

        public void ShowAndActivate()
        {
            // Stop the reset timer if it's running (user is showing window again)
            _resetTimer.Stop();

            Show();
            Activate();
            InputTextBox.Focus();

            // Play fade in animation
            var fadeIn = (Storyboard)FindResource("FadeIn");
            fadeIn.Begin(this);
        }

        private async void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                var query = InputTextBox.Text;
                InputTextBox.Clear();

                // Add user message to panel
                AddMessageToPanel(query, true);

                // Create AI response bubble immediately (empty)
                var aiMessageBubble = CreateMessageBubble("", false);
                ResponsePanel.Children.Add(aiMessageBubble);

                // Animate the message appearance
                aiMessageBubble.Opacity = 0;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                aiMessageBubble.BeginAnimation(OpacityProperty, fadeIn);

                StatusText.Text = "Thinking...";

                try
                {
                    // Get streaming AI response
                    await _aiService.GetStreamingResponseAsync(
                        query,
                        token =>
                        {
                            aiMessageBubble.AppendText(token);
                            // Auto-scroll to keep the latest content visible
                            ScrollToBottom();
                        }, // Called for each token
                        () => StatusText.Text = "Ready"            // Called when complete
                    );
                }
                catch (Exception ex)
                {
                    aiMessageBubble.SetText($"Sorry, I encountered an error: {ex.Message}");
                    StatusText.Text = "Error occurred";
                }
            }
        }

        private void AddMessageToPanel(string message, bool isUser)
        {
            var messageControl = CreateMessageBubble(message, isUser);
            ResponsePanel.Children.Add(messageControl);

            // Animate the message appearance
            messageControl.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            messageControl.BeginAnimation(OpacityProperty, fadeIn);
        }

        private MessageBubble CreateMessageBubble(string message, bool isUser)
        {
            return new MessageBubble
            {
                Message = message,
                IsUser = isUser,
                Margin = new Thickness(0, 5, 0, 5)
            };
        }

        private void ShowLoading(bool show)
        {
            LoadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ScrollToBottom()
        {
            // Find the ScrollViewer parent of the ResponsePanel
            var parent = ResponsePanel.Parent as ScrollViewer;
            parent?.ScrollToBottom();
        }

        private void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isListening)
            {
                StartListening();
            }
            else
            {
                StopListening();
            }
        }

        private void StartListening()
        {
            _isListening = true;
            StatusText.Text = "Listening...";
            VoiceButton.Tag = "listening";

            try
            {
                _speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Voice error: {ex.Message}";
                _isListening = false;
            }
        }

        private void StopListening()
        {
            _isListening = false;
            StatusText.Text = "Ready";
            VoiceButton.Tag = null;
            _speechRecognizer.RecognizeAsyncStop();
        }

        private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Result.Confidence > 0.5)
                {
                    InputTextBox.Text = e.Result.Text;
                    StopListening();
                }
            });
        }

        private void OnRecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _isListening = false;
                StatusText.Text = "Ready";
                VoiceButton.Tag = null;
            });
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Hide window when it loses focus
            HideWindow();
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HideWindow();
            }
        }

        private void HideWindow()
        {
            var fadeOut = (Storyboard)FindResource("FadeOut");
            fadeOut.Completed += (s, e) =>
            {
                Hide();
                // Start the reset timer when window is hidden
                _resetTimer.Start();
            };
            fadeOut.Begin(this);
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Could implement real-time suggestions here
        }

        private void OnResetTimerTick(object? sender, EventArgs e)
        {
            // Stop the timer
            _resetTimer.Stop();

            // Reset the chat by clearing all messages except the welcome messages
            ResetChat();
        }

                private void ResetChat()
        {
            // Clear all messages from the response panel
            ResponsePanel.Children.Clear();

            // Add back the welcome messages
            var welcomeMessage1 = new TextBlock
            {
                Text = "Hi! I'm Cleo, your AI assistant.",
                Foreground = (System.Windows.Media.Brush)FindResource("TextColor"),
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 20, 0, 10)
            };

            var welcomeMessage2 = new TextBlock
            {
                Text = "Type or speak your question, and I'll help you out!",
                Foreground = (System.Windows.Media.Brush)FindResource("SubtleTextColor"),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            ResponsePanel.Children.Add(welcomeMessage1);
            ResponsePanel.Children.Add(welcomeMessage2);
        }
    }
}
