using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class DailyTask
    {
        public int Id { get; set; }
        
        public DateTime Date { get; set; }
        
        public string? Title { get; set; }
        
        public string? Notes { get; set; }
        
        public bool IsCompleted { get; set; } = false;
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

