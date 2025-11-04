using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijitalAjanda.Server.Models
{
    public class TimerSession
    {
        public int Id { get; set; }
        
        public string TaskName { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public int? Duration { get; set; } // minutes
        
        public string SessionType { get; set; } = "pomodoro"; // pomodoro, break, focus
        
        public bool IsCompleted { get; set; } = false;
        
        public string Notes { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public Users User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
