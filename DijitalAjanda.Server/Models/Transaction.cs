using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Type { get; set; } // "income" veya "expense"
        
        [Required]
        public decimal Amount { get; set; }
        
        public string Category { get; set; } // Yemek, Ulaşım, Eğlence, Market, Faturalar, Sağlık, Giyim, Maaş, Diğer
        
        public string Description { get; set; }
        
        public DateTime Date { get; set; } = DateTime.UtcNow;
        
        public bool IsRecurring { get; set; } = false; // Aylık sabit gelir/gider için
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public Users? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
