using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FocusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public FocusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Focus/user/5/date/2024-05-28
        [HttpGet("user/{userId}/date/{date}")]
        public async Task<IActionResult> GetFocusForDay(int userId, DateTime date)
        {
            var start = date.Date;
            var end = date.Date.AddDays(1);
            var records = await _context.Focus
                .Where(f => f.UserId == userId && f.Date >= start && f.Date < end)
                .ToListAsync();
            var totalSeconds = records.Sum(f => f.Duration);
            return Ok(new { totalSeconds, records });
        }

        // POST: api/Focus
        [HttpPost]
        public async Task<IActionResult> CreateFocus([FromBody] Focus focus)
        {
            focus.CreatedAt = DateTime.UtcNow;
            focus.UpdatedAt = DateTime.UtcNow;
            focus.Date = focus.StartTime.Date;
            focus.Duration = (int)(focus.EndTime - focus.StartTime).TotalSeconds;
            _context.Focus.Add(focus);
            await _context.SaveChangesAsync();
            return Ok(focus);
        }

        // GET: api/Focus/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAllForUser(int userId)
        {
            var records = await _context.Focus.Where(f => f.UserId == userId).ToListAsync();
            return Ok(records);
        }
    }
} 