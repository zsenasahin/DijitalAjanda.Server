using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijitalAjanda.Server.Models
{
    public class UserStats
    {
        public int Id { get; set; }
        
        public int GoalsCompleted { get; set; } = 0;
        
        public int HabitsCompleted { get; set; } = 0;
        
        public int ProjectsCompleted { get; set; } = 0;
        
        public int BooksRead { get; set; } = 0;
        
        public int JournalEntries { get; set; } = 0;
        
        public int TotalFocusMinutes { get; set; } = 0;
        
        public int TotalPomodoroSessions { get; set; } = 0;
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public Users User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
