using Microsoft.AspNetCore.Mvc;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using System;

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
        public IActionResult Register(Users user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("Bu email adresiyle zaten bir kullanıcı mevcut.");
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("Kayıt başarılı.");
        }
    }
}
