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
    public class DailyTasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DailyTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserDailyTasks(int userId)
        {
            var dailyTasks = await _context.DailyTasks
                .Where(dt => dt.UserId == userId)
                .Include(dt => dt.TaskItems)
                .OrderByDescending(dt => dt.Date)
                .ToListAsync();

            return Ok(dailyTasks);
        }

        [HttpGet("user/{userId}/date/{date}")]
        public async Task<IActionResult> GetDailyTasksByDate(int userId, DateTime date)
        {
            var dailyTask = await _context.DailyTasks
                .Where(dt => dt.UserId == userId && dt.Date.Date == date.Date)
                .Include(dt => dt.TaskItems)
                .FirstOrDefaultAsync();

            if (dailyTask == null)
            {
                // Create a new daily task for the date if it doesn't exist
                dailyTask = new DailyTask
                {
                    UserId = userId,
                    Date = date.Date,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    TaskItems = new List<TaskItem>()
                };
                _context.DailyTasks.Add(dailyTask);
                await _context.SaveChangesAsync();
            }

            return Ok(dailyTask);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDailyTask(int id)
        {
            var dailyTask = await _context.DailyTasks
                .Include(dt => dt.TaskItems)
                .FirstOrDefaultAsync(dt => dt.Id == id);
            
            if (dailyTask == null)
                return NotFound();

            return Ok(dailyTask);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDailyTask([FromBody] DailyTask dailyTask)
        {
            if (dailyTask.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            dailyTask.CreatedAt = DateTime.UtcNow;
            dailyTask.UpdatedAt = DateTime.UtcNow;

            _context.DailyTasks.Add(dailyTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDailyTask), new { id = dailyTask.Id }, dailyTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDailyTask(int id, [FromBody] DailyTask dailyTask)
        {
            var existingTask = await _context.DailyTasks.FindAsync(id);
            if (existingTask == null)
                return NotFound();

            existingTask.Date = dailyTask.Date;
            existingTask.Notes = dailyTask.Notes;
            existingTask.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDailyTask(int id)
        {
            var dailyTask = await _context.DailyTasks.FindAsync(id);
            if (dailyTask == null)
                return NotFound();

            _context.DailyTasks.Remove(dailyTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/taskitems")]
        public async Task<IActionResult> AddTaskItem(int id, [FromBody] TaskItem taskItem)
        {
            var dailyTask = await _context.DailyTasks.FindAsync(id);
            if (dailyTask == null)
                return NotFound();

            taskItem.DailyTaskId = id;
            taskItem.CreatedAt = DateTime.UtcNow;
            taskItem.UpdatedAt = DateTime.UtcNow;

            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDailyTask), new { id = dailyTask.Id }, taskItem);
        }

        [HttpPut("taskitems/{taskItemId}")]
        public async Task<IActionResult> UpdateTaskItem(int taskItemId, [FromBody] TaskItem taskItem)
        {
            var existingTaskItem = await _context.TaskItems.FindAsync(taskItemId);
            if (existingTaskItem == null)
                return NotFound();

            existingTaskItem.Title = taskItem.Title;
            existingTaskItem.Description = taskItem.Description;
            existingTaskItem.IsCompleted = taskItem.IsCompleted;
            existingTaskItem.Priority = taskItem.Priority;
            existingTaskItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingTaskItem);
        }

        [HttpDelete("taskitems/{taskItemId}")]
        public async Task<IActionResult> DeleteTaskItem(int taskItemId)
        {
            var taskItem = await _context.TaskItems.FindAsync(taskItemId);
            if (taskItem == null)
                return NotFound();

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
