using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using System;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("Bu email adresiyle zaten bir kullanıcı mevcut.");
            }

            var user = new Users
            {
                UserName = request.UserName,
                Email = request.Email
            };

            var passwordHasher = new PasswordHasher<Users>();
            user.Password = passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("Kayıt başarılı.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            var passwordHasher = new PasswordHasher<Users>();
            var result = passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (result == PasswordVerificationResult.Success)
            {
                return Ok(new { userId = user.Id, userName = user.UserName, email = user.Email });
            }
            else
            {
                return Unauthorized("Şifre yanlış.");
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
