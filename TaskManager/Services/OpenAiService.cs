using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using TaskManager.Models; 
using TaskManager.Settings;

namespace TaskManager.Services
{
    public class OpenAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly AiSettings _settings;

        public OpenAiService(HttpClient httpClient, IOptions<AiSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<string> GenerateProjectSummaryAsync(Project project)
        {
            var taskList = project.ProjectTasks != null && project.ProjectTasks.Any()
                ? string.Join(", ", project.ProjectTasks.Select(t => t.Title))
                : "No tasks yet";

            var prompt = $"Te rog să faci un rezumat scurt, de maxim 3 propoziții, pentru proiectul '{project.Title}' care are descrierea '{project.Description}' și următoarele task-uri: {taskList}. Spune dacă proiectul pare aglomerat sau e la început.";

            var requestBody = new
            {
                model = _settings.Model, 
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var bodyString = JsonSerializer.Serialize(requestBody);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            const int maxAttempts = 3;
            var rand = new Random();

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                using var jsonContent = new StringContent(bodyString, Encoding.UTF8, "application/json");

                try
                {
                    var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        if (attempt == maxAttempts)
                        {
                            return "Eroare AI: Limită cereri depășită. Încearcă din nou mai târziu.";
                        }

                        // Respect Retry-After header when present
                        if (response.Headers.TryGetValues("Retry-After", out var values))
                        {
                            var first = values.FirstOrDefault();
                            if (int.TryParse(first, out var seconds))
                            {
                                await Task.Delay(TimeSpan.FromSeconds(seconds));
                            }
                            else
                            {
                                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(rand.Next(0, 500)));
                            }
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(rand.Next(0, 500)));
                        }

                        continue; // retry
                    }

                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    var jsonNode = JsonNode.Parse(responseString);

                    return jsonNode?["choices"]?[0]?["message"]?["content"]?.ToString() ?? "Nu s-a putut genera rezumatul.";
                }
                catch (HttpRequestException ex) when (attempt < maxAttempts)
                {
                    // transient network error, wait and retry
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(rand.Next(0, 500)));
                    continue;
                }
                catch (Exception ex)
                {
                    return $"Eroare AI: {ex.Message}";
                }
            }

            return "Eroare AI: Nu s-a putut comunica cu serviciul AI.";
        }
    }
}