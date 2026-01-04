using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Check()
        {
            var status = new
            {
                Server = "Running",
                Database = "Checking...",
                Time = DateTime.UtcNow
            };

            try
            {
                // Veritabanı bağlantı testi
                if (await _context.Database.CanConnectAsync())
                {
                    return Ok(new { status.Server, Database = "Connected", status.Time });
                }
                else
                {
                    return StatusCode(503, new { status.Server, Database = "Connection Failed", status.Time });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status.Server, Database = $"Error: {ex.Message}", status.Time });
            }
        }
    }
}
