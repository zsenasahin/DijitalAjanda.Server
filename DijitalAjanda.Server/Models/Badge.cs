using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class Badge
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string Icon { get; set; }
        
        public string Category { get; set; } // Habits, Books, Goals, Journal
        
        public int RequiredCount { get; set; } // Required streak/count to earn
        
        public string Color { get; set; } = "#667eea";
    }
    
    public class UserBadge
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        
        public int BadgeId { get; set; }
        [ForeignKey("BadgeId")]
        public Badge? Badge { get; set; }
        
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    }
}
