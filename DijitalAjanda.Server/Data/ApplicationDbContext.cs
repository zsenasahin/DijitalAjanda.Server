
using Microsoft.EntityFrameworkCore;
using DijitalAjanda.Server.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;
using System.Collections.Generic;

namespace DijitalAjanda.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Users> Users { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitCompletion> HabitCompletions { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Events> Events { get; set; }
        public DbSet<Focus> Focus { get; set; }
        public DbSet<Backlog> Backlogs { get; set; }
        public DbSet<DailyTask> DailyTasks { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TimerSession> TimerSessions { get; set; }
        public DbSet<PomodoroSettings> PomodoroSettings { get; set; }
        public DbSet<UserWidget> UserWidgets { get; set; }
        public DbSet<UserStats> UserStats { get; set; }
        public DbSet<KanbanBoard> KanbanBoards { get; set; }
        public DbSet<TaskStatusItem> TaskStatusItems { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure string list conversions for Tags
            var stringListConverter = new ValueConverter<List<string>, string>(
                v => string.Join(',', v),
                v => v.Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToList()
            );

            var stringListComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            // Configure Tags for JournalEntry and Book (List<string>)
            modelBuilder.Entity<JournalEntry>()
                .Property(j => j.Tags)
                .HasConversion(stringListConverter)
                .Metadata.SetValueComparer(stringListComparer);

            modelBuilder.Entity<JournalEntry>()
                .Property(j => j.Images)
                .HasConversion(stringListConverter)
                .Metadata.SetValueComparer(stringListComparer);

            modelBuilder.Entity<Book>()
                .Property(b => b.Tags)
                .HasConversion(stringListConverter)
                .Metadata.SetValueComparer(stringListComparer);

            // Configure relationships
            modelBuilder.Entity<HabitCompletion>()
                .HasOne(hc => hc.Habit)
                .WithMany(h => h.Completions)
                .HasForeignKey(hc => hc.HabitId);

            modelBuilder.Entity<TaskItem>()
                .HasOne(ti => ti.DailyTask)
                .WithMany(dt => dt.TaskItems)
                .HasForeignKey(ti => ti.DailyTaskId);

            modelBuilder.Entity<TaskStatusItem>()
                .HasOne(tsi => tsi.KanbanBoard)
                .WithMany(kb => kb.TaskStatusItems)
                .HasForeignKey(tsi => tsi.KanbanBoardId);

            // Configure unique constraints
            modelBuilder.Entity<UserProfile>()
                .HasIndex(up => up.UserId)
                .IsUnique();

            modelBuilder.Entity<UserStats>()
                .HasIndex(us => us.UserId)
                .IsUnique();

            modelBuilder.Entity<PomodoroSettings>()
                .HasIndex(ps => ps.UserId)
                .IsUnique();

            // Ensure Project Priority and Status are strings
            modelBuilder.Entity<Project>()
                .Property(p => p.Priority)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Project>()
                .Property(p => p.Status)
                .HasColumnType("nvarchar(max)");
        }
    }
}
