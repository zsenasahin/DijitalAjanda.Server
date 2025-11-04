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
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserEvents(int userId)
        {
            var events = await _context.Events
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Time)
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("user/{userId}/date/{date}")]
        public async Task<IActionResult> GetEventsByDate(int userId, DateTime date)
        {
            var events = await _context.Events
                .Where(e => e.UserId == userId && e.Date.Date == date.Date)
                .OrderBy(e => e.Time)
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("user/{userId}/month/{year}/{month}")]
        public async Task<IActionResult> GetEventsByMonth(int userId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var events = await _context.Events
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Time)
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound();

            return Ok(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Events eventItem)
        {
            if (eventItem.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            eventItem.CreatedAt = DateTime.UtcNow;
            eventItem.UpdatedAt = DateTime.UtcNow;

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = eventItem.Id }, eventItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Events eventItem)
        {
            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null)
                return NotFound();

            existingEvent.Title = eventItem.Title;
            existingEvent.Description = eventItem.Description;
            existingEvent.Date = eventItem.Date;
            existingEvent.Time = eventItem.Time;
            existingEvent.Type = eventItem.Type;
            existingEvent.Location = eventItem.Location;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound();

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/type")]
        public async Task<IActionResult> UpdateEventType(int id, [FromBody] EventTypeRequest request)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound();

            eventItem.Type = request.Type;
            eventItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(eventItem);
        }
    }

    public class EventTypeRequest
    {
        public string Type { get; set; }
    }
}
