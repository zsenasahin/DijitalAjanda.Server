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
    public class WidgetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WidgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserWidgets(int userId)
        {
            var widgets = await _context.UserWidgets
                .Where(uw => uw.UserId == userId)
                .OrderBy(uw => uw.Position)
                .ToListAsync();

            return Ok(widgets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWidget(int id)
        {
            var widget = await _context.UserWidgets.FindAsync(id);
            if (widget == null)
                return NotFound();

            return Ok(widget);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWidget([FromBody] UserWidget widget)
        {
            if (widget.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            widget.CreatedAt = DateTime.UtcNow;
            widget.UpdatedAt = DateTime.UtcNow;

            _context.UserWidgets.Add(widget);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWidget), new { id = widget.Id }, widget);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWidget(int id, [FromBody] UserWidget widget)
        {
            var existingWidget = await _context.UserWidgets.FindAsync(id);
            if (existingWidget == null)
                return NotFound();

            existingWidget.WidgetType = widget.WidgetType;
            existingWidget.Position = widget.Position;
            existingWidget.Size = widget.Size;
            existingWidget.IsVisible = widget.IsVisible;
            existingWidget.Settings = widget.Settings;
            existingWidget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingWidget);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWidget(int id)
        {
            var widget = await _context.UserWidgets.FindAsync(id);
            if (widget == null)
                return NotFound();

            _context.UserWidgets.Remove(widget);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/position")]
        public async Task<IActionResult> UpdateWidgetPosition(int id, [FromBody] WidgetPositionRequest request)
        {
            var widget = await _context.UserWidgets.FindAsync(id);
            if (widget == null)
                return NotFound();

            widget.Position = request.Position;
            widget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(widget);
        }

        [HttpPut("{id}/size")]
        public async Task<IActionResult> UpdateWidgetSize(int id, [FromBody] WidgetSizeRequest request)
        {
            var widget = await _context.UserWidgets.FindAsync(id);
            if (widget == null)
                return NotFound();

            widget.Size = request.Size;
            widget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(widget);
        }

        [HttpPut("{id}/visibility")]
        public async Task<IActionResult> ToggleWidgetVisibility(int id)
        {
            var widget = await _context.UserWidgets.FindAsync(id);
            if (widget == null)
                return NotFound();

            widget.IsVisible = !widget.IsVisible;
            widget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(widget);
        }

        [HttpPut("{id}/settings")]
        public async Task<IActionResult> UpdateWidgetSettings(int id, [FromBody] WidgetSettingsRequest request)
        {
            var widget = await _context.UserWidgets.FindAsync(id);
            if (widget == null)
                return NotFound();

            widget.Settings = request.Settings;
            widget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(widget);
        }

        [HttpPost("user/{userId}/reset")]
        public async Task<IActionResult> ResetUserWidgets(int userId)
        {
            var existingWidgets = await _context.UserWidgets
                .Where(uw => uw.UserId == userId)
                .ToListAsync();

            _context.UserWidgets.RemoveRange(existingWidgets);

            // Create default widgets
            var defaultWidgets = new[]
            {
                new UserWidget
                {
                    UserId = userId,
                    WidgetType = "Goals",
                    Position = 1,
                    Size = "medium",
                    IsVisible = true,
                    Settings = "{}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new UserWidget
                {
                    UserId = userId,
                    WidgetType = "Habits",
                    Position = 2,
                    Size = "medium",
                    IsVisible = true,
                    Settings = "{}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new UserWidget
                {
                    UserId = userId,
                    WidgetType = "Timer",
                    Position = 3,
                    Size = "small",
                    IsVisible = true,
                    Settings = "{}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new UserWidget
                {
                    UserId = userId,
                    WidgetType = "Weather",
                    Position = 4,
                    Size = "small",
                    IsVisible = true,
                    Settings = "{}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _context.UserWidgets.AddRange(defaultWidgets);
            await _context.SaveChangesAsync();

            return Ok(defaultWidgets);
        }
    }

    public class WidgetPositionRequest
    {
        public int Position { get; set; }
    }

    public class WidgetSizeRequest
    {
        public string Size { get; set; }
    }

    public class WidgetSettingsRequest
    {
        public string Settings { get; set; }
    }
}
