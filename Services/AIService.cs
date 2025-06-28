using System;
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

            // Assuming the model is running locally on port 11434 (Ollama default)
            _modelEndpoint = "http://localhost:11434/api/generate";
        }

        public async Task<string> GetResponseAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    model = "smollm2:135m-instruct-q3_K_S",
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.7,
                        top_p = 0.9,
                        max_tokens = 500
                    }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_modelEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    dynamic? result = JsonConvert.DeserializeObject(responseJson);
                    return result?.response?.ToString() ?? "I couldn't generate a response.";
                }
                else
                {
                    return $"Error: Unable to connect to the AI model. Make sure Ollama is running with the smollm2 model.";
                }
            }
            catch (HttpRequestException)
            {
                return "Connection error: Please ensure Ollama is running locally on port 11434.";
            }
            catch (TaskCanceledException)
            {
                return "The request timed out. The model might be taking too long to respond.";
            }
            catch (Exception ex)
            {
                return $"An unexpected error occurred: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
