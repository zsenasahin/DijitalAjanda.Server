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
    public class BacklogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BacklogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBacklog(int userId)
        {
            var backlogItems = await _context.Backlogs
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(backlogItems);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBacklogItem(int id)
        {
            var backlogItem = await _context.Backlogs.FindAsync(id);
            if (backlogItem == null)
                return NotFound();

            return Ok(backlogItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBacklogItem([FromBody] Backlog backlog)
        {
            if (backlog.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            backlog.CreatedAt = DateTime.UtcNow;
            backlog.UpdatedAt = DateTime.UtcNow;

            _context.Backlogs.Add(backlog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBacklogItem), new { id = backlog.Id }, backlog);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBacklogItem(int id, [FromBody] Backlog backlog)
        {
            var existingItem = await _context.Backlogs.FindAsync(id);
            if (existingItem == null)
                return NotFound();

            existingItem.Title = backlog.Title;
            existingItem.Description = backlog.Description;
            existingItem.Priority = backlog.Priority;
            existingItem.Status = backlog.Status;
            existingItem.EstimatedEffort = backlog.EstimatedEffort;
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBacklogItem(int id)
        {
            var backlogItem = await _context.Backlogs.FindAsync(id);
            if (backlogItem == null)
                return NotFound();

            _context.Backlogs.Remove(backlogItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBacklogStatus(int id, [FromBody] BacklogStatusRequest request)
        {
            var backlogItem = await _context.Backlogs.FindAsync(id);
            if (backlogItem == null)
                return NotFound();

            backlogItem.Status = request.Status;
            backlogItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(backlogItem);
        }
    }

    public class BacklogStatusRequest
    {
        public string Status { get; set; }
    }
}
