using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        
        public string? DisplayName { get; set; }
        
        public string? Bio { get; set; }
        
        public string? Avatar { get; set; }
        
        public string Theme { get; set; } = "light";
        
        public string Language { get; set; } = "tr";
        
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

