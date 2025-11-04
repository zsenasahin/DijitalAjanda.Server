using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DijitalAjanda.Server.Services
{
    public interface ITimerService
    {
        Task<TimerSession> StartTimerAsync(int userId, string taskName, string sessionType);
        Task<TimerSession> EndTimerAsync(int sessionId, string notes = null);
        Task<TimerSession> PauseTimerAsync(int sessionId);
        Task<TimerSession> ResumeTimerAsync(int sessionId);
        Task<object> GetTimerStatsAsync(int userId);
        Task<PomodoroSettings> GetPomodoroSettingsAsync(int userId);
        Task<PomodoroSettings> UpdatePomodoroSettingsAsync(int userId, PomodoroSettings settings);
        Task<bool> IsTimerRunningAsync(int userId);
    }

    public class TimerService : ITimerService
    {
        private readonly ApplicationDbContext _context;

        public TimerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TimerSession> StartTimerAsync(int userId, string taskName, string sessionType)
        {
            // Check if there's already a running timer
            var existingSession = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && !ts.IsCompleted && ts.EndTime == null)
                .FirstOrDefaultAsync();

            if (existingSession != null)
            {
                throw new InvalidOperationException("Zaten çalışan bir timer var. Önce mevcut timer'ı bitirin.");
            }

            var timerSession = new TimerSession
            {
                UserId = userId,
                TaskName = taskName,
                StartTime = DateTime.UtcNow,
                SessionType = sessionType,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TimerSessions.Add(timerSession);
            await _context.SaveChangesAsync();

            return timerSession;
        }

        public async Task<TimerSession> EndTimerAsync(int sessionId, string notes = null)
        {
            var session = await _context.TimerSessions.FindAsync(sessionId);
            if (session == null)
                throw new ArgumentException("Timer session bulunamadı.");

            if (session.IsCompleted)
                throw new InvalidOperationException("Bu timer session zaten tamamlanmış.");

            session.EndTime = DateTime.UtcNow;
            session.Duration = (int)(session.EndTime.Value - session.StartTime).TotalMinutes;
            session.IsCompleted = true;
            session.Notes = notes;
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<TimerSession> PauseTimerAsync(int sessionId)
        {
            var session = await _context.TimerSessions.FindAsync(sessionId);
            if (session == null)
                throw new ArgumentException("Timer session bulunamadı.");

            if (session.IsCompleted)
                throw new InvalidOperationException("Tamamlanmış timer session duraklatılamaz.");

            // For now, we'll just end the session when paused
            // In a more complex implementation, you might want to track pause/resume times
            return await EndTimerAsync(sessionId, "Duraklatıldı");
        }

        public async Task<TimerSession> ResumeTimerAsync(int sessionId)
        {
            // This would require a more complex implementation with pause/resume tracking
            // For now, we'll create a new session
            var session = await _context.TimerSessions.FindAsync(sessionId);
            if (session == null)
                throw new ArgumentException("Timer session bulunamadı.");

            return await StartTimerAsync(session.UserId, session.TaskName, session.SessionType);
        }

        public async Task<object> GetTimerStatsAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var thisWeek = today.AddDays(-7);
            var thisMonth = today.AddDays(-30);

            var todaySessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.StartTime.Date == today && ts.IsCompleted)
                .ToListAsync();

            var weekSessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.StartTime >= thisWeek && ts.IsCompleted)
                .ToListAsync();

            var monthSessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.StartTime >= thisMonth && ts.IsCompleted)
                .ToListAsync();

            var totalSessions = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.IsCompleted)
                .CountAsync();

            var totalMinutes = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && ts.IsCompleted)
                .SumAsync(ts => ts.Duration ?? 0);

            return new
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
                },
                AllTime = new
                {
                    Sessions = totalSessions,
                    TotalMinutes = totalMinutes,
                    AverageDuration = totalSessions > 0 ? (double)totalMinutes / totalSessions : 0
                }
            };
        }

        public async Task<PomodoroSettings> GetPomodoroSettingsAsync(int userId)
        {
            var settings = await _context.PomodoroSettings
                .FirstOrDefaultAsync(ps => ps.UserId == userId);

            if (settings == null)
            {
                // Create default settings
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

            return settings;
        }

        public async Task<PomodoroSettings> UpdatePomodoroSettingsAsync(int userId, PomodoroSettings settings)
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

            return existingSettings ?? settings;
        }

        public async Task<bool> IsTimerRunningAsync(int userId)
        {
            var runningSession = await _context.TimerSessions
                .Where(ts => ts.UserId == userId && !ts.IsCompleted && ts.EndTime == null)
                .FirstOrDefaultAsync();

            return runningSession != null;
        }
    }
}
