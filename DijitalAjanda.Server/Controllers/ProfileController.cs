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
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
            {
                // Create default profile if doesn't exist
                userProfile = new UserProfile
                {
                    UserId = userId,
                    DisplayName = "",
                    Bio = "",
                    Avatar = "",
                    Theme = "light",
                    Language = "tr",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();
            }

            return Ok(userProfile);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserProfile([FromBody] UserProfile userProfile)
        {
            if (userProfile.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            userProfile.CreatedAt = DateTime.UtcNow;
            userProfile.UpdatedAt = DateTime.UtcNow;

            _context.UserProfiles.Add(userProfile);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserProfile), new { userId = userProfile.UserId }, userProfile);
        }

        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateUserProfile(int userId, [FromBody] UserProfile userProfile)
        {
            var existingProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (existingProfile == null)
            {
                // Create new profile if doesn't exist
                userProfile.UserId = userId;
                userProfile.CreatedAt = DateTime.UtcNow;
                userProfile.UpdatedAt = DateTime.UtcNow;
                _context.UserProfiles.Add(userProfile);
            }
            else
            {
                existingProfile.DisplayName = userProfile.DisplayName;
                existingProfile.Bio = userProfile.Bio;
                existingProfile.Avatar = userProfile.Avatar;
                existingProfile.Theme = userProfile.Theme;
                existingProfile.Language = userProfile.Language;
                existingProfile.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(existingProfile ?? userProfile);
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteUserProfile(int userId)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
                return NotFound();

            _context.UserProfiles.Remove(userProfile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("user/{userId}/theme")]
        public async Task<IActionResult> UpdateTheme(int userId, [FromBody] ThemeRequest request)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
            {
                userProfile = new UserProfile
                {
                    UserId = userId,
                    Theme = request.Theme,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserProfiles.Add(userProfile);
            }
            else
            {
                userProfile.Theme = request.Theme;
                userProfile.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(userProfile);
        }

        [HttpPut("user/{userId}/language")]
        public async Task<IActionResult> UpdateLanguage(int userId, [FromBody] LanguageRequest request)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
            {
                userProfile = new UserProfile
                {
                    UserId = userId,
                    Language = request.Language,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserProfiles.Add(userProfile);
            }
            else
            {
                userProfile.Language = request.Language;
                userProfile.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(userProfile);
        }
    }

    public class ThemeRequest
    {
        public string Theme { get; set; }
    }

    public class LanguageRequest
    {
        public string Language { get; set; }
    }
}
