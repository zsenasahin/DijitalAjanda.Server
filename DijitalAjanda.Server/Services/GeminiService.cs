using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DijitalAjanda.Server.Services
{
    public interface IGeminiService
    {
        Task<string> GenerateCommandJsonAsync(string userMessage, CancellationToken cancellationToken = default);
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private const string ModelName = "gemini-pro";

        public GeminiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateCommandJsonAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                throw new ArgumentException("Message cannot be empty.", nameof(userMessage));

            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("GEMINI_API_KEY environment variable is not set.");

            var systemPrompt = BuildSystemPrompt();

            var url = $"https://generativelanguage.googleapis.com/v1/models/{ModelName}:generateContent?key={apiKey}";

            // Tek bir user mesajı içinde hem sistem prompt'unu hem de kullanıcı mesajını gönder
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = systemPrompt + "\n\nUser: " + userMessage }
                        }
                    }
                }
            };

            using var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Gemini error {(int)response.StatusCode}: {responseText}");

            using var doc = JsonDocument.Parse(responseText);
            var root = doc.RootElement;

            // Gemini: candidates[0].content.parts[0].text
            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                throw new InvalidOperationException("Gemini returned no candidates.");

            var content = candidates[0].GetProperty("content");
            var parts = content.GetProperty("parts");
            if (parts.GetArrayLength() == 0)
                throw new InvalidOperationException("Gemini returned no parts.");

            var text = parts[0].GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Gemini returned empty text.");

            return text;
        }

        private string BuildSystemPrompt()
        {
            var sb = new StringBuilder();

            sb.AppendLine("You are an intent parser for a Turkish productivity app.");
            sb.AppendLine("Your ONLY job is to convert user natural language input into a JSON command.");
            sb.AppendLine("Important hard rules:");
            sb.AppendLine("- NEVER return explanations.");
            sb.AppendLine("- NEVER return markdown.");
            sb.AppendLine("- NEVER include comments.");
            sb.AppendLine("- NEVER return plain text.");
            sb.AppendLine("- ALWAYS return a single valid JSON object.");
            sb.AppendLine("- Output must be valid parsable JSON only.");
            sb.AppendLine();
            sb.AppendLine("Supported intents:");
            sb.AppendLine("- navigate");
            sb.AppendLine("- create_goal");
            sb.AppendLine("- create_habit");
            sb.AppendLine("- create_note");
            sb.AppendLine("- summarize_notes");
            sb.AppendLine("- analyze_habits");
            sb.AppendLine("- analyze_week");
            sb.AppendLine();
            sb.AppendLine("Frontend routes:");
            sb.AppendLine("- /dashboard");
            sb.AppendLine("- /goals");
            sb.AppendLine("- /habits");
            sb.AppendLine("- /notes");
            sb.AppendLine("- /books");
            sb.AppendLine("- /projects");
            sb.AppendLine();
            sb.AppendLine("JSON rules:");
            sb.AppendLine("- Always include \"intent\".");
            sb.AppendLine("- For navigation include \"route\" (one of the supported routes).");
            sb.AppendLine("- For create actions include required fields (for example \"title\", etc.).");
            sb.AppendLine("- Output must be a single JSON object.");
            sb.AppendLine();
            sb.AppendLine("Examples:");
            sb.AppendLine("User: \"Alışkanlıklar ekranına git\"");
            sb.AppendLine("{\"intent\":\"navigate\",\"route\":\"/habits\"}");
            sb.AppendLine();
            sb.AppendLine("User: \"Günde 20 dk kitap oku alışkanlığı ekle\"");
            sb.AppendLine("{\"intent\":\"create_habit\",\"title\":\"Günde 20 dk kitap oku\",\"frequency\":\"daily\"}");
            sb.AppendLine();
            sb.AppendLine("User: \"Bu haftaki notlarımı özetle\"");
            sb.AppendLine("{\"intent\":\"summarize_notes\",\"period\":\"weekly\"}");
            sb.AppendLine();
            sb.AppendLine("Always answer with a single JSON object and nothing else.");

            return sb.ToString();
        }
    }
}


