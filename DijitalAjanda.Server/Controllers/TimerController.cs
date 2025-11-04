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
    public class TimerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTimerSessions(int userId)
        {
            var timerSessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId)
                .OrderByDescending(ts => ts.StartTime)
                .ToListAsync();

            return Ok(timerSessions);
        }

        [HttpGet("user/{userId}/today")]
        public async Task<IActionResult> GetTodayTimerSessions(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var timerSessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.StartTime.Date == today)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();

            return Ok(timerSessions);
        }

        [HttpGet("user/{userId}/week")]
        public async Task<IActionResult> GetWeekTimerSessions(int userId)
        {
            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.Date.DayOfWeek);
            var timerSessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.StartTime >= weekStart)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();

            return Ok(timerSessions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTimerSession(int id)
        {
            var timerSession = await _context.TimerSessions.FindAsync(id);
            if (timerSession == null)
                return NotFound();

            return Ok(timerSession);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTimerSession([FromBody] TimerSession timerSession)
        {
            if (timerSession.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            timerSession.CreatedAt = DateTime.UtcNow;
            timerSession.UpdatedAt = DateTime.UtcNow;

            _context.TimerSessions.Add(timerSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTimerSession), new { id = timerSession.Id }, timerSession);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimerSession(int id, [FromBody] TimerSession timerSession)
        {
            var existingSession = await _context.TimerSessions.FindAsync(id);
            if (existingSession == null)
                return NotFound();

            existingSession.TaskName = timerSession.TaskName;
            existingSession.StartTime = timerSession.StartTime;
            existingSession.EndTime = timerSession.EndTime;
            existingSession.Duration = timerSession.Duration;
            existingSession.SessionType = timerSession.SessionType;
            existingSession.IsCompleted = timerSession.IsCompleted;
            existingSession.Notes = timerSession.Notes;
            existingSession.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingSession);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimerSession(int id)
        {
            var timerSession = await _context.TimerSessions.FindAsync(id);
            if (timerSession == null)
                return NotFound();

            _context.TimerSessions.Remove(timerSession);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/end")]
        public async Task<IActionResult> EndTimerSession(int id, [FromBody] EndTimerRequest request)
        {
            var timerSession = await _context.TimerSessions.FindAsync(id);
            if (timerSession == null)
                return NotFound();

            timerSession.EndTime = DateTime.UtcNow;
            timerSession.Duration = (int)(timerSession.EndTime.Value - timerSession.StartTime).TotalMinutes;
            timerSession.IsCompleted = true;
            timerSession.Notes = request.Notes;
            timerSession.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(timerSession);
        }

        [HttpGet("user/{userId}/stats")]
        public async Task<IActionResult> GetTimerStats(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var thisWeek = today.AddDays(-7);
            var thisMonth = today.AddDays(-30);

            var todaySessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.StartTime.Date == today)
                .ToListAsync();

            var weekSessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.StartTime >= thisWeek)
                .ToListAsync();

            var monthSessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.StartTime >= thisMonth)
                .ToListAsync();

            var stats = new
            {
                Today = new
                {
                    Sessions = todaySessions.Count,
                    TotalMinutes = todaySessions.Sum(ts => ts.Duration ?? 0),
                    AverageDuration = todaySessions.Any() ? todaySessions.Average(ts => ts.Duration ?? 0) : 0
                },
                ThisWeek = new
                {
                    Sessions = weekSessions.Count,
                    TotalMinutes = weekSessions.Sum(ts => ts.Duration ?? 0),
                    AverageDuration = weekSessions.Any() ? weekSessions.Average(ts => ts.Duration ?? 0) : 0
                },
                ThisMonth = new
                {
                    Sessions = monthSessions.Count,
                    TotalMinutes = monthSessions.Sum(ts => ts.Duration ?? 0),
                    AverageDuration = monthSessions.Any() ? monthSessions.Average(ts => ts.Duration ?? 0) : 0
                }
            };

            return Ok(stats);
        }

        [HttpGet("user/{userId}/settings")]
        public async Task<IActionResult> GetPomodoroSettings(int userId)
        {
            var settings = await _context.PomodoroSettings
                .FirstOrDefaultAsync(ps => ps.UserId == userId);

            if (settings == null)
            {
                // Create default settings if doesn't exist
                settings = new PomodoroSettings
                {
                    UserId = userId,
                    WorkDuration = 25,
                    ShortBreakDuration = 5,
                    LongBreakDuration = 15,
                    SessionsUntilLongBreak = 4,
                    AutoStartBreaks = false,
                    AutoStartPomodoros = false,
                    SoundEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PomodoroSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return Ok(settings);
        }

        [HttpPut("user/{userId}/settings")]
        public async Task<IActionResult> UpdatePomodoroSettings(int userId, [FromBody] PomodoroSettings settings)
        {
            var existingSettings = await _context.PomodoroSettings
                .FirstOrDefaultAsync(ps => ps.UserId == userId);

            if (existingSettings == null)
            {
                settings.UserId = userId;
                settings.CreatedAt = DateTime.UtcNow;
                settings.UpdatedAt = DateTime.UtcNow;
                _context.PomodoroSettings.Add(settings);
            }
            else
            {
            existingSettings.WorkDuration = settings.WorkDuration;
                existingSettings.ShortBreakDuration = settings.ShortBreakDuration;
            existingSettings.LongBreakDuration = settings.LongBreakDuration;
                existingSettings.SessionsUntilLongBreak = settings.SessionsUntilLongBreak;
            existingSettings.AutoStartBreaks = settings.AutoStartBreaks;
            existingSettings.AutoStartPomodoros = settings.AutoStartPomodoros;
                existingSettings.SoundEnabled = settings.SoundEnabled;
            existingSettings.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(existingSettings ?? settings);
        }
    }

    public class EndTimerRequest
    {
        public string Notes { get; set; }
    }
}
