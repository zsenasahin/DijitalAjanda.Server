using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HabitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HabitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Habit>>> GetUserHabits(int userId)
        {
            var habits = await _context.Habits
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return Ok(habits);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Habit>> GetHabit(int id)
        {
            var habit = await _context.Habits.FindAsync(id);
            if (habit == null)
                return NotFound();

            return Ok(habit);
        }

        [HttpPost]
        public async Task<ActionResult<Habit>> CreateHabit([FromBody] Habit habit)
        {
            // Frontend'den gelen UserId'yi kullan
            if (habit.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            habit.CreatedAt = DateTime.UtcNow;
            habit.UpdatedAt = DateTime.UtcNow;

            _context.Habits.Add(habit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHabit), new { id = habit.Id }, habit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabit(int id, [FromBody] Habit habit)
        {
            var existingHabit = await _context.Habits.FindAsync(id);
            if (existingHabit == null)
                return NotFound();

            existingHabit.Title = habit.Title;
            existingHabit.Description = habit.Description;
            existingHabit.Type = habit.Type;
            existingHabit.Category = habit.Category;
            existingHabit.Icon = habit.Icon;
            existingHabit.Color = habit.Color;
            existingHabit.StartDate = habit.StartDate;
            existingHabit.EndDate = habit.EndDate;
            existingHabit.TargetFrequency = habit.TargetFrequency;
            existingHabit.FrequencyUnit = habit.FrequencyUnit;
            existingHabit.IsActive = habit.IsActive;
            existingHabit.ReminderTime = habit.ReminderTime;
            existingHabit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingHabit);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabit(int id)
        {
            var habit = await _context.Habits.FindAsync(id);
            if (habit == null)
                return NotFound();

            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteHabit(int id, [FromBody] CompleteHabitRequest request)
        {
            var habit = await _context.Habits.FindAsync(id);
            if (habit == null)
                return NotFound();

            var completion = new HabitCompletion
            {
                HabitId = id,
                //CompletedAt = request.Date,
                Count = request.Count,
                Notes = request.Notes
            };

            _context.HabitCompletions.Add(completion);
            await _context.SaveChangesAsync();

            return Ok(completion);
        }

        [HttpDelete("{id}/completion/{completionId}")]
        public async Task<IActionResult> RemoveCompletion(int id, int completionId)
        {
            var completion = await _context.HabitCompletions
                .FirstOrDefaultAsync(hc => hc.HabitId == id && hc.Id == completionId);

            if (completion == null)
                return NotFound();

            _context.HabitCompletions.Remove(completion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetHabitStats(int id)
        {
            var habit = await _context.Habits
                .Include(h => h.Completions)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (habit == null)
                return NotFound();

            var today = DateTime.UtcNow.Date;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var stats = new
            {
                TotalCompletions = habit.Completions.Count,
                TodayCompletions = habit.Completions.Count(c => c.CompletedAt.Date == today),
                ThisWeekCompletions = habit.Completions.Count(c => c.CompletedAt >= thisWeek),
                ThisMonthCompletions = habit.Completions.Count(c => c.CompletedAt >= thisMonth),
                Streak = CalculateStreak(habit.Completions),
                CompletionRate = CalculateCompletionRate(habit)
            };

            return Ok(stats);
        }

        private int CalculateStreak(List<HabitCompletion> completions)
        {
            var sortedCompletions = completions
                .OrderByDescending(c => c.CompletedAt)
                .ToList();

            if (!sortedCompletions.Any())
                return 0;

            var streak = 0;
            var currentDate = DateTime.Today;

            foreach (var completion in sortedCompletions)
            {
                if (completion.CompletedAt.Date == currentDate)
                {
                    streak++;
                    currentDate = currentDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        private decimal CalculateCompletionRate(Habit habit)
        {
            if (habit.StartDate > DateTime.UtcNow.Date)
                return 0;

            var daysSinceStart = (DateTime.Today - habit.StartDate).Days + 1;
            var totalCompletions = habit.Completions.Sum(c => c.Count);
            var targetCompletions = daysSinceStart * habit.TargetFrequency;

            if (targetCompletions == 0)
                return 0;

            return Math.Min(100, ((decimal)totalCompletions / (decimal)targetCompletions) * 100);
        }
    }

    public class CompleteHabitRequest
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public int Count { get; set; } = 1;
        public string Notes { get; set; }
    }
}
