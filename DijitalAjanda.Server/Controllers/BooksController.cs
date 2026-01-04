using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string RecommendationApiUrl = "http://localhost:5001/recommend";

        public BooksController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
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
            try
            {
                // Frontend'den gelen UserId'yi kullan
                if (book.UserId <= 0)
                {
                    return BadRequest($"Kullanıcı ID'si gerekli. Gelen UserId: {book.UserId}");
                }

                // Boş string'leri null'a çevir
                if (string.IsNullOrWhiteSpace(book.Author))
                    book.Author = null;
                if (string.IsNullOrWhiteSpace(book.ISBN))
                    book.ISBN = null;
                if (string.IsNullOrWhiteSpace(book.Description))
                    book.Description = null;
                if (string.IsNullOrWhiteSpace(book.Review))
                    book.Review = null;
                if (string.IsNullOrWhiteSpace(book.CoverImage))
                    book.CoverImage = null;

                // Tags için null kontrolü
                if (book.Tags == null)
                {
                    book.Tags = new List<string>();
                }
                
                book.CreatedAt = DateTime.UtcNow;
                book.UpdatedAt = DateTime.UtcNow;

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen veritabanı hatası";
                return BadRequest($"Kitap oluşturulurken veritabanı hatası: {innerMessage}");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return BadRequest($"Kitap oluşturulurken hata: {ex.Message}. Inner: {innerMessage}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
                return NotFound();

            existingBook.Title = book.Title;
            existingBook.Author = string.IsNullOrWhiteSpace(book.Author) ? null : book.Author;
            existingBook.ISBN = string.IsNullOrWhiteSpace(book.ISBN) ? null : book.ISBN;
            existingBook.Description = string.IsNullOrWhiteSpace(book.Description) ? null : book.Description;
            existingBook.TotalPages = book.TotalPages;
            existingBook.CurrentPage = book.CurrentPage;
            existingBook.Status = book.Status;
            existingBook.Rating = book.Rating;
            existingBook.Review = string.IsNullOrWhiteSpace(book.Review) ? null : book.Review;
            existingBook.StartedDate = book.StartedDate;
            existingBook.FinishedDate = book.FinishedDate;
            existingBook.CoverImage = string.IsNullOrWhiteSpace(book.CoverImage) ? null : book.CoverImage;
            existingBook.Tags = book.Tags ?? new List<string>();
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

        [HttpPost("user/{userId}/recommendations")]
        public async Task<IActionResult> GetRecommendations(int userId, [FromBody] RecommendationRequest request)
        {
            try
            {
                // Kullanıcının okuduğu kitapları al (CurrentlyReading ve Completed)
                var userBooks = await _context.Books
                    .Where(b => b.UserId == userId && 
                           (b.Status == "CurrentlyReading" || b.Status == "Completed"))
                    .ToListAsync();

                // Eğer kullanıcının okuduğu kitap yoksa ve "use_my_books" true ise hata döndür
                if (request.UseMyBooks && userBooks.Count == 0)
                {
                    return BadRequest(new { error = "Öneri almak için en az bir kitap okumuş olmalısınız." });
                }

                // Python API'ye gönderilecek veri
                var recommendationData = new
                {
                    user_books = request.UseMyBooks 
                        ? userBooks.Select(b => new
                        {
                            title = b.Title ?? "",
                            author = b.Author ?? "",
                            tags = b.Tags ?? new List<string>(),
                            description = b.Description ?? ""
                        }).Cast<object>().ToList() 
                        : new List<object>(),
                    user_genres = request.Genres ?? new List<string>(),
                    filters = new
                    {
                        genre = request.FilterGenre,
                        author_type = request.FilterAuthorType
                    },
                    n_recommendations = request.NumberOfRecommendations
                };

                // Python API'ye istek at
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var jsonContent = JsonSerializer.Serialize(recommendationData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(RecommendationApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { error = $"Öneri API hatası: {errorContent}" });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var recommendations = JsonSerializer.Deserialize<RecommendationResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return Ok(recommendations);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = $"Öneri servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin. Detay: {ex.Message}" });
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, new { error = "Öneri servisi zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Öneri alınırken bir hata oluştu: {ex.Message}" });
            }
        }
    }

    public class RecommendationRequest
    {
        public bool UseMyBooks { get; set; } = true;
        public List<string>? Genres { get; set; }
        public string? FilterGenre { get; set; }
        public string? FilterAuthorType { get; set; } // "turkish", "foreign", or null
        public int NumberOfRecommendations { get; set; } = 10;
    }

    public class RecommendationResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("recommendations")]
        public List<RecommendedBook>? Recommendations { get; set; }
        
        [JsonPropertyName("count")]
        public int Count { get; set; }
        
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    public class RecommendedBook
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";
        
        [JsonPropertyName("author")]
        public string Author { get; set; } = "";
        
        [JsonPropertyName("genre")]
        public string Genre { get; set; } = "";
        
        [JsonPropertyName("rating")]
        public double Rating { get; set; }
        
        [JsonPropertyName("pages")]
        public int Pages { get; set; }
        
        [JsonPropertyName("publisher")]
        public string Publisher { get; set; } = "";
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
        
        [JsonPropertyName("similarity_score")]
        public double SimilarityScore { get; set; }
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
