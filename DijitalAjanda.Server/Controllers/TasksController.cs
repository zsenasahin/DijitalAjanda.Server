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
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTasks(int userId)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.DailyTask.UserId == userId)
                .Include(t => t.DailyTask)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("user/{userId}/status/{status}")]
        public async Task<IActionResult> GetTasksByStatus(int userId, string status)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.DailyTask.UserId == userId && t.Status == status)
                .Include(t => t.DailyTask)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("user/{userId}/priority/{priority}")]
        public async Task<IActionResult> GetTasksByPriority(int userId, string priority)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.DailyTask.UserId == userId && t.Priority == priority)
                .Include(t => t.DailyTask)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _context.TaskItems
                .Include(t => t.DailyTask)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskItem task)
        {
            if (task.DailyTaskId <= 0)
            {
                return BadRequest("Günlük görev ID'si gerekli");
            }
            
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItem task)
        {
            var existingTask = await _context.TaskItems.FindAsync(id);
            if (existingTask == null)
                return NotFound();

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.IsCompleted = task.IsCompleted;
            existingTask.Priority = task.Priority;
            existingTask.Status = task.Status;
            existingTask.DueDate = task.DueDate;
            existingTask.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] TaskStatusRequest request)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            task.Status = request.Status;
            task.UpdatedAt = DateTime.UtcNow;

            if (request.Status == "completed")
            {
                task.IsCompleted = true;
            }
            else if (request.Status == "todo" || request.Status == "inprogress")
            {
                task.IsCompleted = false;
            }

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpPut("{id}/priority")]
        public async Task<IActionResult> UpdateTaskPriority(int id, [FromBody] TaskPriorityRequest request)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            task.Priority = request.Priority;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> ToggleTaskCompletion(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            task.IsCompleted = !task.IsCompleted;
            task.Status = task.IsCompleted ? "completed" : "todo";
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpGet("user/{userId}/today")]
        public async Task<IActionResult> GetTodayTasks(int userId)
        {
            var today = DateTime.Today;
            var tasks = await _context.TaskItems
                .Where(t => t.DailyTask.UserId == userId && 
                           t.DailyTask.Date.Date == today)
                .Include(t => t.DailyTask)
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("user/{userId}/overdue")]
        public async Task<IActionResult> GetOverdueTasks(int userId)
        {
            var today = DateTime.Today;
            var tasks = await _context.TaskItems
                .Where(t => t.DailyTask.UserId == userId && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value.Date < today && 
                           !t.IsCompleted)
                .Include(t => t.DailyTask)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return Ok(tasks);
        }
    }

    public class TaskStatusRequest
    {
        public string Status { get; set; }
    }

    public class TaskPriorityRequest
    {
        public string Priority { get; set; }
    }
}
