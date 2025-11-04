using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class Project
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public string Status { get; set; } = "Planned";
        
        public string Priority { get; set; } = "Medium";
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public decimal Progress { get; set; } = 0; // 0-100
        
        public string Color { get; set; } = "#6366f1";
        
        public string Icon { get; set; } = "??";
        
        public string Tags { get; set; } = "";
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
