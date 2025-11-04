using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijitalAjanda.Server.Models
{
    public class UserWidget
    {
        public int Id { get; set; }
        
        [Required]
        public string WidgetType { get; set; }
        
        public int Position { get; set; } = 0;
        
        public string Size { get; set; } = "medium"; // small, medium, large
        
        public bool IsVisible { get; set; } = true;
        
        public string Settings { get; set; } = "{}";
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public Users User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
