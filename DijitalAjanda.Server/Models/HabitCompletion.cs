using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijitalAjanda.Server.Models
{
    public class HabitCompletion
    {
        [Key]
        public int Id { get; set; }

        public DateTime CompletedAt { get; set; }

        public int Count { get; set; } = 1;

        public string? Notes { get; set; }

        [Required]
        public int HabitId { get; set; }

        [ForeignKey("HabitId")]
        public Habit? Habit { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
