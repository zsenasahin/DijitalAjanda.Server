using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class Note
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public string Color { get; set; } = "#fef3c7"; // Varsayılan sarı not rengi
        
        public bool IsPinned { get; set; } = false;
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
