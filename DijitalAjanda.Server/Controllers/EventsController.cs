using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Events>>> GetEvents()
        {
            return await _context.Events.Include(e => e.User).ToListAsync();
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Events>> GetEvent(int id)
        {
            var @event = await _context.Events.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<Events>> CreateEvent(Events @event)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if (@event == null)
                {
                    return BadRequest(new { message = "Event data is null" });
                }

                if (string.IsNullOrEmpty(@event.Title))
                {
                    return BadRequest(new { message = "Title is required" });
                }

                if (@event.Start == default(DateTime))
                {
                    return BadRequest(new { message = "Start date is required" });
                }

                if (@event.End == default(DateTime))
                {
                    return BadRequest(new { message = "End date is required" });
                }

                if (@event.Start > @event.End)
                {
                    return BadRequest(new { message = "Start date cannot be after end date" });
                }

                if (string.IsNullOrEmpty(@event.Category))
                {
                    return BadRequest(new { message = "Category is required" });
                }

                if (@event.UserId <= 0)
                {
                    return BadRequest(new { message = "Valid UserId is required" });
                }

                @event.CreatedAt = DateTime.UtcNow;
                @event.UpdatedAt = DateTime.UtcNow;
                
                _context.Events.Add(@event);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, @event);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Database error occurred", details = ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating event", details = ex.ToString() });
            }
        }

        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, Events @event)
        {
            if (id != @event.Id)
            {
                return BadRequest();
            }

            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
} 