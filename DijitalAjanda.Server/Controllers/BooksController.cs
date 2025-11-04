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
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBooks(int userId)
        {
            var books = await _context.Books
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(books);
        }

        [HttpGet("user/{userId}/status/{status}")]
        public async Task<IActionResult> GetBooksByStatus(int userId, string status)
        {
            var books = await _context.Books
                .Where(b => b.UserId == userId && b.Status == status)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            // Frontend'den gelen UserId'yi kullan
            if (book.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
                return NotFound();

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.ISBN = book.ISBN;
            existingBook.Description = book.Description;
            existingBook.TotalPages = book.TotalPages;
            existingBook.CurrentPage = book.CurrentPage;
            existingBook.Status = book.Status;
            existingBook.Rating = book.Rating;
            existingBook.Review = book.Review;
            existingBook.StartedDate = book.StartedDate;
            existingBook.FinishedDate = book.FinishedDate;
            existingBook.CoverImage = book.CoverImage;
            existingBook.Tags = book.Tags;
            existingBook.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingBook);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/progress")]
        public async Task<IActionResult> UpdateReadingProgress(int id, [FromBody] ReadingProgressRequest request)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            book.CurrentPage = request.CurrentPage;
            book.UpdatedAt = DateTime.UtcNow;

            if (book.TotalPages.HasValue && book.CurrentPage >= book.TotalPages)
            {
                book.Status = "Completed";
                book.FinishedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(book);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookStatus(int id, [FromBody] BookStatusRequest request)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            book.Status = request.Status;
            book.UpdatedAt = DateTime.UtcNow;

            if (request.Status == "CurrentlyReading" && !book.StartedDate.HasValue)
            {
                book.StartedDate = DateTime.UtcNow;
            }
            else if (request.Status == "Completed")
            {
                book.FinishedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(book);
        }
    }

    public class ReadingProgressRequest
    {
        public int CurrentPage { get; set; }
    }

    public class BookStatusRequest
    {
        public string Status { get; set; }
    }
}
