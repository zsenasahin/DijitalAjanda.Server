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
            try
            {
                var tasks = await _context.TaskItems
                    .Include(t => t.DailyTask)
                    .Include(t => t.Project)
                    .Where(t => t.DailyTask != null && t.DailyTask.UserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Görevler yüklenirken bir hata oluştu", error = ex.Message });
            }
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
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { message = "Görev başlığı gereklidir" });
                }

                if (request.UserId <= 0)
                {
                    return BadRequest(new { message = "Kullanıcı ID'si gereklidir" });
                }

                // Eğer DailyTaskId verilmemişse ama UserId varsa, bugünün DailyTask'ini bul veya oluştur
                int dailyTaskId = request.DailyTaskId ?? 0;
                
                if (dailyTaskId <= 0 && request.UserId > 0)
                {
                    var today = DateTime.Today;
                    var dailyTask = await _context.DailyTasks
                        .FirstOrDefaultAsync(dt => dt.UserId == request.UserId && dt.Date.Date == today);
                    
                    if (dailyTask == null)
                    {
                        dailyTask = new DailyTask
                        {
                            UserId = request.UserId,
                            Date = today,
                            Title = "Günlük Görevler",
                            Notes = null,
                            IsCompleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.DailyTasks.Add(dailyTask);
                        await _context.SaveChangesAsync();
                    }
                    
                    dailyTaskId = dailyTask.Id;
                }
                
                if (dailyTaskId <= 0)
                {
                    return BadRequest(new { message = "Günlük görev ID'si veya Kullanıcı ID'si gerekli" });
                }
                
                var task = new TaskItem
                {
                    Title = request.Title.Trim(),
                    Description = request.Description ?? null,
                    Priority = !string.IsNullOrWhiteSpace(request.Priority) ? request.Priority.ToLower() : "medium",
                    Status = !string.IsNullOrWhiteSpace(request.Status) ? request.Status.ToLower() : "todo",
                    DueDate = request.DueDate,
                    ProjectId = request.ProjectId > 0 ? request.ProjectId : null,
                    DailyTaskId = dailyTaskId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsCompleted = false
                };

                _context.TaskItems.Add(task);
                await _context.SaveChangesAsync();

                // Include ile tekrar yükle
                var createdTask = await _context.TaskItems
                    .Include(t => t.DailyTask)
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == task.Id);

                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, createdTask);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, new { 
                    message = "Görev oluşturulurken bir hata oluştu", 
                    error = innerException,
                    details = dbEx.ToString()
                });
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { 
                    message = "Görev oluşturulurken bir hata oluştu", 
                    error = innerException,
                    details = ex.ToString()
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdateRequest request)
        {
            try
            {
                var existingTask = await _context.TaskItems.FindAsync(id);
                if (existingTask == null)
                    return NotFound();

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { message = "Görev başlığı gereklidir" });
                }

                existingTask.Title = request.Title.Trim();
                existingTask.Description = request.Description ?? string.Empty;
                existingTask.Priority = request.Priority?.ToLower() ?? existingTask.Priority;
                existingTask.Status = request.Status?.ToLower() ?? existingTask.Status;
                existingTask.DueDate = request.DueDate;
                existingTask.ProjectId = request.ProjectId;
                existingTask.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Include ile tekrar yükle
                var updatedTask = await _context.TaskItems
                    .Include(t => t.DailyTask)
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == id);

                return Ok(updatedTask);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Görev güncellenirken bir hata oluştu", error = ex.Message });
            }
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

            // Status'u küçük harfe çevir
            var statusLower = request.Status?.ToLower() ?? "todo";
            task.Status = statusLower;
            task.UpdatedAt = DateTime.UtcNow;

            if (statusLower == "done" || statusLower == "completed")
            {
                task.IsCompleted = true;
            }
            else if (statusLower == "todo" || statusLower == "inprogress" || statusLower == "review")
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

    public class TaskCreateRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }
        public int? DailyTaskId { get; set; }
        public int? ProjectId { get; set; }
        public int UserId { get; set; }
    }

    public class TaskUpdateRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }
        public int? ProjectId { get; set; }
    }
}
