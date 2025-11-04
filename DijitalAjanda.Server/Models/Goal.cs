using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class Goal
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string Type { get; set; } // Daily, Weekly, Monthly, Yearly - string olarak
        
        [Required]
        public string Category { get; set; } // Personal, Work, Health, Learning, etc. - string olarak
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public string Status { get; set; } = "NotStarted"; // string olarak
        
        public int Priority { get; set; } = 1; // 1-5
        
        public decimal? TargetValue { get; set; } // For numeric goals (e.g., 30 books)
        public decimal? CurrentValue { get; set; } = 0;
        public string Unit { get; set; } // e.g., "books", "km", "hours"
        
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; }
        
        public string Color { get; set; } = "#6366f1";
        public string Icon { get; set; } = "🎯";
        
        public string Tags { get; set; } = ""; // List<string> yerine string olarak
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
