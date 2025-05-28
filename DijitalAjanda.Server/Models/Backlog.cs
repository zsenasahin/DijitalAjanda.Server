using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijitalAjanda.Server.Models
{
    public class Backlog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Users? User { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public int? ParentId { get; set; } // Ana görevse NULL, alt görevse ana görevin Id'si

        [ForeignKey("ParentId")]
        public Backlog? Parent { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }
    }
} 