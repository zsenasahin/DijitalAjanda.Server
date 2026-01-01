using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DijitalAjanda.Server.Services
{
    public interface IOllamaService
    {
        Task<string> GenerateJsonCommandAsync(string userMessage, CancellationToken cancellationToken = default);
    }

    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _httpClient;

        // Model adı appsettings.json üzerinden de okunabilir ama şimdilik sabit tutuyoruz
        private const string ModelName = "llama3";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public OllamaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:11434");
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<string> GenerateJsonCommandAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                throw new ArgumentException("User message cannot be empty", nameof(userMessage));
            }

            var systemPrompt = BuildSystemPrompt();

            var requestBody = new
            {
                model = ModelName,
                stream = false,
                prompt = $"{systemPrompt}\n\nUser: {userMessage}\nAssistant:"
            };

            using var response = await _httpClient.PostAsJsonAsync("/api/generate", requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Ollama generate failed with status {(int)response.StatusCode}: {errorText}");
            }

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);

            try
            {
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                // Ollama response format: { "response": "...", "done": true, ... }
                if (root.TryGetProperty("response", out var responseElement))
                {
                    return responseElement.GetString() ?? string.Empty;
                }

                // Bazı sürümlerde farklı olabilir, fallback olarak tüm json'ı döndür
                return jsonString;
            }
            catch (JsonException)
            {
                // Beklenmeyen format, ham cevabı döndür
                return jsonString;
            }
        }

        private string BuildSystemPrompt()
        {
            // Burada kuralları çok net ve katı veriyoruz
            var sb = new StringBuilder();

            sb.Append("You are an intent parser for a Turkish productivity app.\n");
            sb.Append("Your ONLY job is to convert user natural language input into a JSON command.\n");
            sb.Append("NEVER return explanations.\n");
            sb.Append("NEVER return plain text.\n");
            sb.Append("NEVER use markdown.\n");
            sb.Append("NEVER include comments.\n");
            sb.Append("ALWAYS return a single valid JSON object.\n");
            sb.Append("If the user input is unrelated, still return a valid JSON with a best-effort intent.\n\n");

            sb.Append("Supported intents:\n");
            sb.Append("- navigate\n");
            sb.Append("- create_goal\n");
            sb.Append("- create_habit\n");
            sb.Append("- create_note\n");
            sb.Append("- summarize_notes\n");
            sb.Append("- analyze_habits\n");
            sb.Append("- analyze_week\n\n");

            sb.Append("Frontend routes:\n");
            sb.Append("- /dashboard\n");
            sb.Append("- /goals\n");
            sb.Append("- /habits\n");
            sb.Append("- /notes\n");
            sb.Append("- /books\n");
            sb.Append("- /projects\n\n");

            sb.Append("JSON rules:\n");
            sb.Append("- Always include \"intent\".\n");
            sb.Append("- For navigation include \"route\" with one of the supported routes.\n");
            sb.Append("- For create actions include required fields like \"title\" and any obvious fields (e.g. frequency).\n");
            sb.Append("- For summarize or analyze intents include a simple \"period\" or other obvious fields.\n");
            sb.Append("- No markdown.\n");
            sb.Append("- No comments.\n");
            sb.Append("- No explanation text.\n");
            sb.Append("- Output must be valid parsable JSON.\n\n");

            sb.Append("Examples:\n");
            sb.Append("User: \"Alışkanlıklar ekranına git\"\n");
            sb.Append("{\"intent\":\"navigate\",\"route\":\"/habits\"}\n\n");

            sb.Append("User: \"Günde 20 dk kitap oku alışkanlığı ekle\"\n");
            sb.Append("{\"intent\":\"create_habit\",\"title\":\"Günde 20 dk kitap oku\",\"frequency\":\"daily\"}\n\n");

            sb.Append("User: \"Bu haftaki notlarımı özetle\"\n");
            sb.Append("{\"intent\":\"summarize_notes\",\"period\":\"weekly\"}\n\n");

            sb.Append("Always answer with a single JSON object and nothing else.");

            return sb.ToString();
        }
    }
}


