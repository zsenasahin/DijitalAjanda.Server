using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string? Content { get; set; }
        
        public string? Mood { get; set; } // 😊 😢 😡 😴 😐 etc.
        
        public int MoodScore { get; set; } = 5; // 1-10
        
        public string? Weather { get; set; }
        
        public string? Location { get; set; }
        
        public List<string> Tags { get; set; } = new List<string>();
        
        public List<string> Images { get; set; } = new List<string>(); // Image URLs
        
        public bool IsPrivate { get; set; } = false;
        
        public string? Password { get; set; } // Şifreli günlük için
        
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Günlük yazısının duygu durum analizi sonucu (Navigation Property)
        /// </summary>
        public JournalSentiment? Sentiment { get; set; }
    }
}
