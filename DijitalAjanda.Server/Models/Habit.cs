
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class Habit
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string? Description { get; set; }
        
        public HabitType Type { get; set; } = HabitType.Daily;
        
        public HabitCategory Category { get; set; } = HabitCategory.Personal;
        
        public string Icon { get; set; } = "🔄";
        
        public string Color { get; set; } = "#10b981";
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public int TargetFrequency { get; set; } = 1; // How many times per period
        
        public string FrequencyUnit { get; set; } = "day"; // day, week, month
        
        public List<HabitCompletion> Completions { get; set; } = new List<HabitCompletion>();
        
        public bool IsActive { get; set; } = true;
        
        public string ReminderTime { get; set; } // HH:mm format
                
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    
    public enum HabitType
    {
        Daily,
        Weekly,
        Monthly,
        Custom
    }
    
    public enum HabitCategory
    {
        Personal,
        Health,
        Work,
        Learning,
        Fitness,
        Mindfulness,
        Other
    }
} 