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
    public class GoalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GoalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserGoals(int userId)
        {
            var goals = await _context.Goals
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return Ok(goals);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGoal(int id)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
                return NotFound();

            return Ok(goal);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGoal([FromBody] Goal goal)
        {
            try
            {
                // Frontend'den gelen UserId'yi kullan
                if (goal.UserId <= 0)
                {
                    return BadRequest($"Kullanıcı ID'si gerekli. Gelen UserId: {goal.UserId}");
                }
                
                goal.CreatedAt = DateTime.UtcNow;
                goal.UpdatedAt = DateTime.UtcNow;

                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, goal);
            }
            catch (Exception ex)
            {
                return BadRequest($"Hedef oluşturulurken hata: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGoal(int id, [FromBody] Goal goal)
        {
            var existingGoal = await _context.Goals.FindAsync(id);
            if (existingGoal == null)
                return NotFound();

            existingGoal.Title = goal.Title;
            existingGoal.Description = goal.Description;
            existingGoal.Type = goal.Type;
            existingGoal.Category = goal.Category;
            existingGoal.StartDate = goal.StartDate;
            existingGoal.EndDate = goal.EndDate;
            existingGoal.Status = goal.Status;
            existingGoal.Priority = goal.Priority;
            existingGoal.TargetValue = goal.TargetValue;
            existingGoal.CurrentValue = goal.CurrentValue;
            existingGoal.Unit = goal.Unit;
            existingGoal.IsCompleted = goal.IsCompleted;
            existingGoal.CompletedDate = goal.CompletedDate;
            existingGoal.Color = goal.Color;
            existingGoal.Icon = goal.Icon;
            existingGoal.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingGoal);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
                return NotFound();

            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/progress")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] ProgressUpdateRequest request)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
                return NotFound();

            goal.CurrentValue = request.CurrentValue;
            goal.UpdatedAt = DateTime.UtcNow;

            if (goal.TargetValue.HasValue && goal.CurrentValue >= goal.TargetValue)
            {
                goal.IsCompleted = true;
                goal.CompletedDate = DateTime.UtcNow;
                goal.Status = "Completed"; // Enum yerine string kullan
            }

            await _context.SaveChangesAsync();

            return Ok(goal);
        }
    }

    public class ProgressUpdateRequest
    {
        public decimal CurrentValue { get; set; }
    }
}
