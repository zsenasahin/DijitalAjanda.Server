using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijitalAjanda.Server.Models
{
    public class KanbanBoard
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public Users User { get; set; }
        
        public virtual ICollection<TaskStatusItem> TaskStatusItems { get; set; } = new List<TaskStatusItem>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
