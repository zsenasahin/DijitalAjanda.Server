using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijitalAjanda.Server.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public bool IsCompleted { get; set; } = false;
        
        public string Priority { get; set; } = "medium";
        
        public string Status { get; set; } = "todo";
        
        public DateTime? DueDate { get; set; }
        
        public int DailyTaskId { get; set; }
        [ForeignKey("DailyTaskId")]
        public DailyTask DailyTask { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
