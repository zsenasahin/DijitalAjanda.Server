using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    /// <summary>
    /// Günlük yazılarının duygu durum analizi sonuçlarını saklayan entity.
    /// JournalEntry ile 1:1 ilişkisi vardır.
    /// </summary>
    public class JournalSentiment
    {
        public int Id { get; set; }
        
        /// <summary>
        /// İlişkili günlük yazısının ID'si (Foreign Key)
        /// </summary>
        [Required]
        public int JournalEntryId { get; set; }
        
        /// <summary>
        /// İlişkili günlük yazısı (Navigation Property)
        /// </summary>
        [ForeignKey("JournalEntryId")]
        [JsonIgnore]
        public JournalEntry? JournalEntry { get; set; }
        
        /// <summary>
        /// Duygu durumu etiketi: "Positive", "Negative", "Neutral"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string SentimentLabel { get; set; } = "Neutral";
        
        /// <summary>
        /// Duygu durumu skoru (0.0 - 1.0 arası)
        /// 0.0 = Çok Negatif, 0.5 = Nötr, 1.0 = Çok Pozitif
        /// </summary>
        public float SentimentScore { get; set; } = 0.5f;
        
        /// <summary>
        /// Analizin yapıldığı tarih/saat
        /// </summary>
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }
}
