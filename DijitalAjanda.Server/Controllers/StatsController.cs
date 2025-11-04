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
    public class StatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}/overview")]
        public async Task<IActionResult> GetUserStats(int userId)
        {
            var today = DateTime.Today;
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

            var stats = new
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

            return Ok(stats);
        }

        [HttpGet("user/{userId}/goals")]
        public async Task<IActionResult> GetGoalStats(int userId)
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

            var goalStats = new
            {
                TotalGoals = totalGoals,
                CompletedGoals = completedGoals,
                CompletionRate = totalGoals > 0 ? (double)completedGoals / totalGoals * 100 : 0,
                StatusBreakdown = goals
            };

            return Ok(goalStats);
        }

        [HttpGet("user/{userId}/habits")]
        public async Task<IActionResult> GetHabitStats(int userId)
        {
            var today = DateTime.Today;
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

            var habitStats = new
            {
                TotalHabits = totalHabits,
                CompletedToday = completedHabitsToday,
                CompletedThisWeek = completedHabitsThisWeek,
                CompletedThisMonth = completedHabitsThisMonth,
                DailyCompletionRate = totalHabits > 0 ? (double)completedHabitsToday / totalHabits * 100 : 0
            };

            return Ok(habitStats);
        }

        [HttpGet("user/{userId}/projects")]
        public async Task<IActionResult> GetProjectStats(int userId)
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

            var projectStats = new
            {
                TotalProjects = totalProjects,
                CompletedProjects = completedProjects,
                CompletionRate = totalProjects > 0 ? (double)completedProjects / totalProjects * 100 : 0,
                StatusBreakdown = projects
            };

            return Ok(projectStats);
        }

        [HttpGet("user/{userId}/books")]
        public async Task<IActionResult> GetBookStats(int userId)
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

            var bookStats = new
            {
                TotalBooks = totalBooks,
                BooksRead = booksRead,
                CompletionRate = totalBooks > 0 ? (double)booksRead / totalBooks * 100 : 0,
                StatusBreakdown = books
            };

            return Ok(bookStats);
        }

        [HttpGet("user/{userId}/journal")]
        public async Task<IActionResult> GetJournalStats(int userId)
        {
            var today = DateTime.Today;
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

            var journalStats = new
            {
                TotalEntries = totalEntries,
                EntriesThisWeek = entriesThisWeek,
                EntriesThisMonth = entriesThisMonth,
                WeeklyAverage = thisWeek.DayOfYear > 0 ? (double)entriesThisWeek / 7 : 0,
                MonthlyAverage = thisMonth.DayOfYear > 0 ? (double)entriesThisMonth / 30 : 0
            };

            return Ok(journalStats);
        }
    }
}
