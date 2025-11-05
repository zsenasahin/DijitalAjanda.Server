
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class Book
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string? Author { get; set; }
        
        public string? ISBN { get; set; }
        
        public string? Description { get; set; }
        
        public int? TotalPages { get; set; }
        
        public int? CurrentPage { get; set; } = 0;
        
        public string Status { get; set; } = "ToRead";
        
        public int Rating { get; set; } = 0;
        
        public string? Review { get; set; }
        
        public DateTime? StartedDate { get; set; }
        
        public DateTime? FinishedDate { get; set; }
        
        public string? CoverImage { get; set; }
        
        public List<string> Tags { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
