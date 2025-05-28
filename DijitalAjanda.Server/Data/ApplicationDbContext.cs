using Microsoft.EntityFrameworkCore;
using DijitalAjanda.Server.Models;

namespace DijitalAjanda.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Users> Users { get; set; }
        public DbSet<Events> Events { get; set; }
        public DbSet<Focus> Focus { get; set; }
        public DbSet<Backlog> Backlog { get; set; }
        public DbSet<Goals> Goals { get; set; }
    }
}
