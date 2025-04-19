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
    }
}
