using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GaraShowcase.Api.Models
{
    public class Job
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = "Engineering"; // Engineering, Business, Design, Marketing

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Requirements { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Open"; // Open, Closed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
