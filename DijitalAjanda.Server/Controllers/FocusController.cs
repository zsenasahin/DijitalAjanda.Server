using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFocusSessions(int userId)
        {
            var focusSessions = await _context.Focus
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.StartTime)
                .ToListAsync();

            return Ok(focusSessions);
        }

        [HttpGet("user/{userId}/today")]
        public async Task<IActionResult> GetTodayFocusSessions(int userId)
        {
            var today = DateTime.Today;
            var focusSessions = await _context.Focus
                .Where(f => f.UserId == userId && f.StartTime.Date == today)
                .OrderBy(f => f.StartTime)
                .ToListAsync();

            return Ok(focusSessions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFocusSession(int id)
        {
            var focusSession = await _context.Focus.FindAsync(id);
            if (focusSession == null)
                return NotFound();

            return Ok(focusSession);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFocusSession([FromBody] Focus focusSession)
        {
            if (focusSession.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            focusSession.CreatedAt = DateTime.UtcNow;
            focusSession.UpdatedAt = DateTime.UtcNow;

            _context.Focus.Add(focusSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFocusSession), new { id = focusSession.Id }, focusSession);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFocusSession(int id, [FromBody] Focus focusSession)
        {
            var existingSession = await _context.Focus.FindAsync(id);
            if (existingSession == null)
                return NotFound();

            existingSession.Task = focusSession.Task;
            existingSession.StartTime = focusSession.StartTime;
            existingSession.EndTime = focusSession.EndTime;
            existingSession.Duration = focusSession.Duration;
            existingSession.Distractions = focusSession.Distractions;
            existingSession.Notes = focusSession.Notes;
            existingSession.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingSession);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFocusSession(int id)
        {
            var focusSession = await _context.Focus.FindAsync(id);
            if (focusSession == null)
                return NotFound();

            _context.Focus.Remove(focusSession);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/end")]
        public async Task<IActionResult> EndFocusSession(int id, [FromBody] EndFocusRequest request)
        {
            var focusSession = await _context.Focus.FindAsync(id);
            if (focusSession == null)
                return NotFound();

            focusSession.EndTime = DateTime.UtcNow;
            focusSession.Duration = (int)(focusSession.EndTime.Value - focusSession.StartTime).TotalMinutes;
            focusSession.Distractions = request.Distractions;
            focusSession.Notes = request.Notes;
            focusSession.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(focusSession);
        }

        [HttpGet("user/{userId}/stats")]
        public async Task<IActionResult> GetFocusStats(int userId)
        {
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-7);
            var thisMonth = today.AddDays(-30);

            var todaySessions = await _context.Focus
                .Where(f => f.UserId == userId && f.StartTime.Date == today)
                .ToListAsync();

            var weekSessions = await _context.Focus
                .Where(f => f.UserId == userId && f.StartTime >= thisWeek)
                .ToListAsync();

            var monthSessions = await _context.Focus
                .Where(f => f.UserId == userId && f.StartTime >= thisMonth)
                .ToListAsync();

            var stats = new
            {
                Today = new
                {
                    Sessions = todaySessions.Count,
                    TotalMinutes = todaySessions.Sum(f => f.Duration ?? 0),
                    AverageDuration = todaySessions.Any() ? todaySessions.Average(f => f.Duration ?? 0) : 0
                },
                ThisWeek = new
                {
                    Sessions = weekSessions.Count,
                    TotalMinutes = weekSessions.Sum(f => f.Duration ?? 0),
                    AverageDuration = weekSessions.Any() ? weekSessions.Average(f => f.Duration ?? 0) : 0
                },
                ThisMonth = new
                {
                    Sessions = monthSessions.Count,
                    TotalMinutes = monthSessions.Sum(f => f.Duration ?? 0),
                    AverageDuration = monthSessions.Any() ? monthSessions.Average(f => f.Duration ?? 0) : 0
                }
            };

            return Ok(stats);
        }
    }

    public class EndFocusRequest
    {
        public int Distractions { get; set; }
        public string Notes { get; set; }
    }
}
