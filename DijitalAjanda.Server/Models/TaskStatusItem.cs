using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijitalAjanda.Server.Models
{
    public class TaskStatusItem
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string Color { get; set; } = "#f4f4f4";
        
        public int Position { get; set; } = 0;
        
        public int KanbanBoardId { get; set; }
        [ForeignKey("KanbanBoardId")]
        public KanbanBoard KanbanBoard { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
