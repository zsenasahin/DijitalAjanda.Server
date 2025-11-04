using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijitalAjanda.Server.Models
{
    public class PomodoroSettings
    {
        public int Id { get; set; }
        
        public int WorkDuration { get; set; } = 25; // minutes
        
        public int ShortBreakDuration { get; set; } = 5; // minutes
        
        public int LongBreakDuration { get; set; } = 15; // minutes
        
        public int SessionsUntilLongBreak { get; set; } = 4;
        
        public bool AutoStartBreaks { get; set; } = false;
        
        public bool AutoStartPomodoros { get; set; } = false;
        
        public bool SoundEnabled { get; set; } = true;
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public Users User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
