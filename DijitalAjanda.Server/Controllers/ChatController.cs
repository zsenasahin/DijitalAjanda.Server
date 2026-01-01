using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Server.Services;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public ChatController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        public class ChatRequest
        {
            public string Message { get; set; }
            public string CurrentPath { get; set; }
            public int? UserId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "message is required" });
            }

            string raw;
            try
            {
                raw = await _geminiService.GenerateCommandJsonAsync(request.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Gemini call failed", details = ex.Message });
            }

            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return BadRequest(new { error = "Model response is not a JSON object", raw });
                }

                if (!root.TryGetProperty("intent", out var intentProp) || intentProp.ValueKind != JsonValueKind.String)
                {
                    return BadRequest(new { error = "Model response does not contain a valid 'intent' field", raw });
                }

                // Ham JSON'u doğrudan döndür
                return Content(root.GetRawText(), "application/json");
            }
            catch (JsonException)
            {
                return BadRequest(new { error = "Model returned invalid JSON", raw });
            }
        }
    }
}

