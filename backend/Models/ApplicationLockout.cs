using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GaraShowcase.Api.Models
{
    public class ApplicationLockout
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [ForeignKey("StudentId")]
        public User? Student { get; set; }

        [Required]
        public Guid JobId { get; set; }

        [ForeignKey("JobId")]
        public Job? Job { get; set; }

        [Required]
        public DateTime LockedUntil { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
