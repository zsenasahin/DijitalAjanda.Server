using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kullanıcının tüm notlarını getir
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Note>>> GetUserNotes(int userId)
        {
            return await _context.Notes
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // Tek not getir
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            return note;
        }

        // Yeni not ekle
        [HttpPost]
        public async Task<ActionResult<Note>> CreateNote(Note note)
        {
            note.CreatedAt = DateTime.UtcNow;
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
        }

        // Not güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, Note note)
        {
            if (id != note.Id)
            {
                return BadRequest();
            }

            note.UpdatedAt = DateTime.UtcNow;
            _context.Entry(note).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // Notu sabitle/kaldır
        [HttpPut("{id}/pin")]
        public async Task<IActionResult> TogglePinNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            note.IsPinned = !note.IsPinned;
            note.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(note);
        }

        // Not sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Not ara
        [HttpGet("user/{userId}/search")]
        public async Task<ActionResult<IEnumerable<Note>>> SearchNotes(int userId, [FromQuery] string query)
        {
            return await _context.Notes
                .Where(n => n.UserId == userId && n.Content.Contains(query))
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
    }
}
