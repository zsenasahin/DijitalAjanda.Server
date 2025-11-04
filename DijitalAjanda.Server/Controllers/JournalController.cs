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
    public class JournalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JournalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserEntries(int userId)
        {
            var entries = await _context.JournalEntries
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.Date)
                .ToListAsync();

            return Ok(entries);
        }

        [HttpGet("user/{userId}/date/{date}")]
        public async Task<IActionResult> GetEntryByDate(int userId, DateTime date)
        {
            var entry = await _context.JournalEntries
                .FirstOrDefaultAsync(j => j.UserId == userId && j.Date.Date == date.Date);

            return Ok(entry);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntry(int id)
        {
            var entry = await _context.JournalEntries.FindAsync(id);
            if (entry == null)
                return NotFound();

            return Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntry([FromBody] JournalEntry entry)
        {
            // Frontend'den gelen UserId'yi kullan
            if (entry.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, entry);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntry(int id, [FromBody] JournalEntry entry)
        {
            var existingEntry = await _context.JournalEntries.FindAsync(id);
            if (existingEntry == null)
                return NotFound();

            existingEntry.Title = entry.Title;
            existingEntry.Content = entry.Content;
            existingEntry.Mood = entry.Mood;
            existingEntry.MoodScore = entry.MoodScore;
            existingEntry.Weather = entry.Weather;
            existingEntry.Location = entry.Location;
            existingEntry.Tags = entry.Tags;
            existingEntry.Images = entry.Images;
            existingEntry.IsPrivate = entry.IsPrivate;
            existingEntry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingEntry);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var entry = await _context.JournalEntries.FindAsync(id);
            if (entry == null)
                return NotFound();

            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

