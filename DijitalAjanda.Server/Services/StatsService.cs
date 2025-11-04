using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DijitalAjanda.Server.Services
{
    public interface IStatsService
    {
        Task<object> GetUserStatsAsync(int userId);
        Task<object> GetGoalStatsAsync(int userId);
        Task<object> GetHabitStatsAsync(int userId);
        Task<object> GetProjectStatsAsync(int userId);
        Task<object> GetBookStatsAsync(int userId);
        Task<object> GetJournalStatsAsync(int userId);
        Task<object> GetFocusStatsAsync(int userId);
        Task<object> GetTimerStatsAsync(int userId);
    }

    public class StatsService : IStatsService
    {
        private readonly ApplicationDbContext _context;

        public StatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetUserStatsAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var thisWeek = today.AddDays(-7);
            var thisMonth = today.AddDays(-30);

            // Goals stats
            var totalGoals = await _context.Goals
                .Where(g => g.UserId == userId)
                .CountAsync();

            var completedGoals = await _context.Goals
                .Where(g => g.UserId == userId && g.Status == "Completed")
                .CountAsync();

            // Habits stats
            var totalHabits = await _context.Habits
                .Where(h => h.UserId == userId)
                .CountAsync();

            var completedHabitsToday = await _context.HabitCompletions
                .Where(hc => hc.Habit.UserId == userId && hc.CompletedAt.Date == today)
                .CountAsync();

            // Projects stats
            var totalProjects = await _context.Projects
                .Where(p => p.UserId == userId)
                .CountAsync();

            var activeProjects = await _context.Projects
                .Where(p => p.UserId == userId && p.Status == "InProgress")
                .CountAsync();

            // Books stats
            var totalBooks = await _context.Books
                .Where(b => b.UserId == userId)
                .CountAsync();

            var booksRead = await _context.Books
                .Where(b => b.UserId == userId && b.Status == "Completed")
                .CountAsync();

            // Journal entries
            var journalEntriesThisWeek = await _context.JournalEntries
                .Where(j => j.UserId == userId && j.CreatedAt >= thisWeek)
                .CountAsync();

            var journalEntriesThisMonth = await _context.JournalEntries
                .Where(j => j.UserId == userId && j.CreatedAt >= thisMonth)
                .CountAsync();

            return new
            {
                Goals = new
                {
                    Total = totalGoals,
                    Completed = completedGoals,
                    CompletionRate = totalGoals > 0 ? (double)completedGoals / totalGoals * 100 : 0
                },
                Habits = new
                {
                    Total = totalHabits,
                    CompletedToday = completedHabitsToday,
                    CompletionRate = totalHabits > 0 ? (double)completedHabitsToday / totalHabits * 100 : 0
                },
                Projects = new
                {
                    Total = totalProjects,
                    Active = activeProjects,
                    CompletionRate = totalProjects > 0 ? (double)(totalProjects - activeProjects) / totalProjects * 100 : 0
                },
                Books = new
                {
                    Total = totalBooks,
                    Read = booksRead,
                    CompletionRate = totalBooks > 0 ? (double)booksRead / totalBooks * 100 : 0
                },
                Journal = new
                {
                    EntriesThisWeek = journalEntriesThisWeek,
                    EntriesThisMonth = journalEntriesThisMonth
                }
            };
        }

        public async Task<object> GetGoalStatsAsync(int userId)
        {
            var goals = await _context.Goals
                .Where(g => g.UserId == userId)
                .GroupBy(g => g.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalGoals = await _context.Goals
                .Where(g => g.UserId == userId)
                .CountAsync();

            var completedGoals = await _context.Goals
                .Where(g => g.UserId == userId && g.Status == "Completed")
                .CountAsync();

            return new
            {
                TotalGoals = totalGoals,
                CompletedGoals = completedGoals,
                CompletionRate = totalGoals > 0 ? (double)completedGoals / totalGoals * 100 : 0,
                StatusBreakdown = goals
            };
        }

        public async Task<object> GetHabitStatsAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var thisWeek = today.AddDays(-7);
            var thisMonth = today.AddDays(-30);

            var totalHabits = await _context.Habits
                .Where(h => h.UserId == userId)
                .CountAsync();

            var completedHabitsToday = await _context.HabitCompletions
                .Where(hc => hc.Habit.UserId == userId && hc.CompletedAt.Date == today)
                .CountAsync();

            var completedHabitsThisWeek = await _context.HabitCompletions
                .Where(hc => hc.Habit.UserId == userId && hc.CompletedAt >= thisWeek)
                .CountAsync();

            var completedHabitsThisMonth = await _context.HabitCompletions
                .Where(hc => hc.Habit.UserId == userId && hc.CompletedAt >= thisMonth)
                .CountAsync();

            return new
            {
                TotalHabits = totalHabits,
                CompletedToday = completedHabitsToday,
                CompletedThisWeek = completedHabitsThisWeek,
                CompletedThisMonth = completedHabitsThisMonth,
                DailyCompletionRate = totalHabits > 0 ? (double)completedHabitsToday / totalHabits * 100 : 0
            };
        }

        public async Task<object> GetProjectStatsAsync(int userId)
        {
            var projects = await _context.Projects
                .Where(p => p.UserId == userId)
                .GroupBy(p => p.Status)
                .Select(p => new { Status = p.Key, Count = p.Count() })
                .ToListAsync();

            var totalProjects = await _context.Projects
                .Where(p => p.UserId == userId)
                .CountAsync();

            var completedProjects = await _context.Projects
                .Where(p => p.UserId == userId && p.Status == "Completed")
                .CountAsync();

            return new
            {
                TotalProjects = totalProjects,
                CompletedProjects = completedProjects,
                CompletionRate = totalProjects > 0 ? (double)completedProjects / totalProjects * 100 : 0,
                StatusBreakdown = projects
            };
        }

        public async Task<object> GetBookStatsAsync(int userId)
        {
            var books = await _context.Books
                .Where(b => b.UserId == userId)
                .GroupBy(b => b.Status)
                .Select(b => new { Status = b.Key, Count = b.Count() })
                .ToListAsync();

            var totalBooks = await _context.Books
                .Where(b => b.UserId == userId)
                .CountAsync();

            var booksRead = await _context.Books
                .Where(b => b.UserId == userId && b.Status == "Completed")
                .CountAsync();

            return new
            {
                TotalBooks = totalBooks,
                BooksRead = booksRead,
                CompletionRate = totalBooks > 0 ? (double)booksRead / totalBooks * 100 : 0,
                StatusBreakdown = books
            };
        }

        public async Task<object> GetJournalStatsAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var thisWeek = today.AddDays(-7);
            var thisMonth = today.AddDays(-30);

            var totalEntries = await _context.JournalEntries
                .Where(j => j.UserId == userId)
                .CountAsync();

            var entriesThisWeek = await _context.JournalEntries
                .Where(j => j.UserId == userId && j.CreatedAt >= thisWeek)
                .CountAsync();

            var entriesThisMonth = await _context.JournalEntries
                .Where(j => j.UserId == userId && j.CreatedAt >= thisMonth)
                .CountAsync();

            return new
            {
                TotalEntries = totalEntries,
                EntriesThisWeek = entriesThisWeek,
                EntriesThisMonth = entriesThisMonth,
                WeeklyAverage = thisWeek.DayOfYear > 0 ? (double)entriesThisWeek / 7 : 0,
                MonthlyAverage = thisMonth.DayOfYear > 0 ? (double)entriesThisMonth / 30 : 0
            };
        }

        public async Task<object> GetFocusStatsAsync(int userId)
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

            return new
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
        }

        public async Task<object> GetTimerStatsAsync(int userId)
        {
            var today = DateTime.Today;
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
                }
            };
        }
    }
}
