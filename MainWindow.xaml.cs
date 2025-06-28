using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Speech.Recognition;
using Cleo.Services;
using Cleo.Controls;

namespace Cleo
{
    public partial class MainWindow : Window
    {
        private readonly AIService _aiService;
        private readonly SpeechRecognitionEngine _speechRecognizer;
        private bool _isListening = false;

        public MainWindow()
        {
            InitializeComponent();
            _aiService = new AIService();
            _speechRecognizer = InitializeSpeechRecognition();
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
            fadeOut.Completed += (s, e) => Hide();
            fadeOut.Begin(this);
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Could implement real-time suggestions here
        }
    }
}
