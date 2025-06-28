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

                // Show loading
                ShowLoading(true);
                StatusText.Text = "Thinking...";

                try
                {
                    // Get AI response
                    var response = await _aiService.GetResponseAsync(query);

                    // Add AI response to panel
                    AddMessageToPanel(response, false);
                    StatusText.Text = "Ready";
                }
                catch (Exception ex)
                {
                    AddMessageToPanel($"Sorry, I encountered an error: {ex.Message}", false);
                    StatusText.Text = "Error occurred";
                }
                finally
                {
                    ShowLoading(false);
                }
            }
        }

        private void AddMessageToPanel(string message, bool isUser)
        {
            var messageControl = new MessageBubble
            {
                Message = message,
                IsUser = isUser,
                Margin = new Thickness(0, 5, 0, 5)
            };

            ResponsePanel.Children.Add(messageControl);

            // Animate the message appearance
            messageControl.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            messageControl.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void ShowLoading(bool show)
        {
            LoadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
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
