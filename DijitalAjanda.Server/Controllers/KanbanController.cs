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
    public class KanbanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KanbanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserKanbanBoards(int userId)
        {
            var kanbanBoards = await _context.KanbanBoards
                .Where(kb => kb.UserId == userId)
                .Include(kb => kb.TaskStatusItems)
                .OrderByDescending(kb => kb.CreatedAt)
                .ToListAsync();

            return Ok(kanbanBoards);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetKanbanBoard(int id)
        {
            var kanbanBoard = await _context.KanbanBoards
                .Include(kb => kb.TaskStatusItems)
                .FirstOrDefaultAsync(kb => kb.Id == id);

            if (kanbanBoard == null)
                return NotFound();

            return Ok(kanbanBoard);
        }

        [HttpPost]
        public async Task<IActionResult> CreateKanbanBoard([FromBody] KanbanBoard kanbanBoard)
        {
            if (kanbanBoard.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            kanbanBoard.CreatedAt = DateTime.UtcNow;
            kanbanBoard.UpdatedAt = DateTime.UtcNow;

            _context.KanbanBoards.Add(kanbanBoard);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetKanbanBoard), new { id = kanbanBoard.Id }, kanbanBoard);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKanbanBoard(int id, [FromBody] KanbanBoard kanbanBoard)
        {
            var existingBoard = await _context.KanbanBoards.FindAsync(id);
            if (existingBoard == null)
                return NotFound();

            existingBoard.Name = kanbanBoard.Name;
            existingBoard.Description = kanbanBoard.Description;
            existingBoard.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingBoard);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKanbanBoard(int id)
        {
            var kanbanBoard = await _context.KanbanBoards.FindAsync(id);
            if (kanbanBoard == null)
                return NotFound();

            _context.KanbanBoards.Remove(kanbanBoard);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{boardId}/columns")]
        public async Task<IActionResult> AddColumn(int boardId, [FromBody] TaskStatusItem column)
        {
            var kanbanBoard = await _context.KanbanBoards.FindAsync(boardId);
            if (kanbanBoard == null)
                return NotFound();

            column.KanbanBoardId = boardId;
            column.CreatedAt = DateTime.UtcNow;
            column.UpdatedAt = DateTime.UtcNow;

            _context.TaskStatusItems.Add(column);
            await _context.SaveChangesAsync();

            return Ok(column);
        }

        [HttpPut("columns/{columnId}")]
        public async Task<IActionResult> UpdateColumn(int columnId, [FromBody] TaskStatusItem column)
        {
            var existingColumn = await _context.TaskStatusItems.FindAsync(columnId);
            if (existingColumn == null)
                return NotFound();

            existingColumn.Name = column.Name;
            existingColumn.Description = column.Description;
            existingColumn.Color = column.Color;
            existingColumn.Position = column.Position;
            existingColumn.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingColumn);
        }

        [HttpDelete("columns/{columnId}")]
        public async Task<IActionResult> DeleteColumn(int columnId)
        {
            var column = await _context.TaskStatusItems.FindAsync(columnId);
            if (column == null)
                return NotFound();

            _context.TaskStatusItems.Remove(column);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("columns/{columnId}/move")]
        public async Task<IActionResult> MoveColumn(int columnId, [FromBody] MoveColumnRequest request)
        {
            var column = await _context.TaskStatusItems.FindAsync(columnId);
            if (column == null)
                return NotFound();

            column.Position = request.NewPosition;
            column.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(column);
        }
    }

    public class MoveColumnRequest
    {
        public int NewPosition { get; set; }
    }
}
