using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cleo.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelEndpoint;

        public AIService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Use the chat endpoint for proper streaming support
            _modelEndpoint = "http://localhost:11434/api/chat";
        }

        public async Task GetStreamingResponseAsync(string prompt, Action<string> onTokenReceived, Action onComplete)
        {
            try
            {
                var request = new
                {
                    model = "smollm2:135m-instruct-q8_0",
                    messages = new[]
                    {
                        new { role = "system", content = "You are Cleo, an intelligent AI assistant. Be helpful, polite, and concise. If unsure, say so rather than guess. Maintain a warm, approachable tone." },
                        new { role = "user", content = prompt }
                    },
                    stream = true,
                    options = new
                    {
                        temperature = 0.3, // Lower temperature for more consistent responses
                        top_p = 0.8,
                        top_k = 20,
                        num_predict = 300,
                        repeat_penalty = 1.1
                    }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await _httpClient.PostAsync(_modelEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);

                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            dynamic? result = JsonConvert.DeserializeObject(line);
                            var token = result?.message?.content?.ToString();

                            if (!string.IsNullOrEmpty(token))
                            {
                                onTokenReceived(token);
                            }

                            // Check if this is the final response
                            if (result?.done == true)
                            {
                                onComplete();
                                break;
                            }
                        }
                        catch (JsonException)
                        {
                            // Skip malformed JSON lines
                            continue;
                        }
                    }
                }
                else
                {
                    onTokenReceived($"Error: Unable to connect to the AI model. Make sure Ollama is running with the smollm2 model.");
                    onComplete();
                }
            }
            catch (HttpRequestException)
            {
                onTokenReceived("Connection error: Please ensure Ollama is running locally on port 11434.");
                onComplete();
            }
            catch (TaskCanceledException)
            {
                onTokenReceived("The request timed out. The model might be taking too long to respond.");
                onComplete();
            }
            catch (Exception ex)
            {
                onTokenReceived($"An unexpected error occurred: {ex.Message}");
                onComplete();
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
