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
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserProjects(int userId)
        {
            var projects = await _context.Projects
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("user/{userId}/status/{status}")]
        public async Task<IActionResult> GetProjectsByStatus(int userId, string status)
        {
            var projects = await _context.Projects
                .Where(p => p.UserId == userId && p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project project)
        {
            // Frontend'den gelen UserId'yi kullan
            if (project.UserId <= 0)
            {
                return BadRequest("Kullanıcı ID'si gerekli");
            }
            
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] Project project)
        {
            var existingProject = await _context.Projects.FindAsync(id);
            if (existingProject == null)
                return NotFound();

            existingProject.Title = project.Title;
            existingProject.Description = project.Description;
            existingProject.Status = project.Status;
            existingProject.Priority = project.Priority;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.Progress = project.Progress;
            existingProject.Color = project.Color;
            existingProject.Icon = project.Icon;
            existingProject.Tags = project.Tags;
            existingProject.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existingProject);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/progress")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] ProjectProgressRequest request)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            project.Progress = request.Progress;
            project.UpdatedAt = DateTime.UtcNow;

            if (project.Progress >= 100)
            {
                project.Status = "Completed";
            }

            await _context.SaveChangesAsync();

            return Ok(project);
        }
    }

    public class ProjectProgressRequest
    {
        public decimal Progress { get; set; }
    }
}

