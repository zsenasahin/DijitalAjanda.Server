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
            
            // Date'i UTC'ye çevir
            if (entry.Date.Kind == DateTimeKind.Unspecified)
            {
                entry.Date = DateTime.SpecifyKind(entry.Date, DateTimeKind.Utc);
            }
            else
            {
                entry.Date = entry.Date.ToUniversalTime();
            }
            
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.User = null; // Navigation property'yi null yap

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

            // Date'i UTC'ye çevir
            if (entry.Date.Kind == DateTimeKind.Unspecified)
            {
                existingEntry.Date = DateTime.SpecifyKind(entry.Date, DateTimeKind.Utc);
            }
            else
            {
                existingEntry.Date = entry.Date.ToUniversalTime();
            }
            
            existingEntry.Title = entry.Title;
            existingEntry.Content = entry.Content ?? string.Empty;
            existingEntry.Mood = entry.Mood ?? string.Empty;
            existingEntry.MoodScore = entry.MoodScore;
            existingEntry.Weather = entry.Weather ?? string.Empty;
            existingEntry.Location = entry.Location ?? string.Empty;
            existingEntry.Tags = entry.Tags ?? new List<string>();
            existingEntry.Images = entry.Images ?? new List<string>();
            existingEntry.IsPrivate = entry.IsPrivate;
            
            // Password güncellemesi: Eğer yeni password verilmişse güncelle, yoksa mevcut password'ü koru
            if (!string.IsNullOrEmpty(entry.Password))
            {
                existingEntry.Password = entry.Password;
            }
            // Eğer isPrivate false yapıldıysa password'ü temizle
            else if (!entry.IsPrivate)
            {
                existingEntry.Password = null;
            }
            // Aksi halde mevcut password'ü koru (hiçbir şey yapma)
            
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

