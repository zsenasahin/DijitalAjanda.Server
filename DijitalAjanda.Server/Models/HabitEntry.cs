using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijitalAjanda.Server.Models
{
    /// <summary>
    /// Her bir alışkanlık slotu için kayıt.
    /// Örnek: 7 haftalık, haftada 1 = 7 adet HabitEntry
    /// </summary>
    public class HabitEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int HabitId { get; set; }

        [ForeignKey("HabitId")]
        [JsonIgnore]
        public Habit? Habit { get; set; }

        /// <summary>
        /// Kaçıncı slot (1, 2, 3...)
        /// </summary>
        public int SlotNumber { get; set; }

        /// <summary>
        /// Bu slot'un planlandığı tarih
        /// </summary>
        public DateTime ScheduledDate { get; set; }

        /// <summary>
        /// Tamamlandı mı?
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Tamamlanma zamanı (null ise henüz tamamlanmamış)
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
