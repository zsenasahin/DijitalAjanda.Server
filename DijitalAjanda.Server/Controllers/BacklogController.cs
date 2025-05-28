using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
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

        // GET: api/Backlog/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAllForUser(int userId)
        {
            var records = await _context.Backlog
                .Where(b => b.UserId == userId)
                .OrderBy(b => b.ParentId)
                .ThenBy(b => b.Id)
                .ToListAsync();
            return Ok(records);
        }

        // POST: api/Backlog
        [HttpPost]
        public async Task<IActionResult> CreateBacklog([FromBody] Backlog backlog)
        {
            backlog.CreatedAt = DateTime.UtcNow;
            backlog.UpdatedAt = DateTime.UtcNow;
            _context.Backlog.Add(backlog);
            await _context.SaveChangesAsync();
            return Ok(backlog);
        }

        // PUT: api/Backlog/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBacklog(int id, [FromBody] Backlog backlog)
        {
            var existing = await _context.Backlog.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Title = backlog.Title;
            existing.Start = backlog.Start;
            existing.End = backlog.End;
            existing.ParentId = backlog.ParentId;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE: api/Backlog/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBacklog(int id)
        {
            var existing = await _context.Backlog.FindAsync(id);
            if (existing == null) return NotFound();
            _context.Backlog.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
} 