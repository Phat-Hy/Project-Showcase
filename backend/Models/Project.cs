using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GaraShowcase.Api.Models
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(150, MinimumLength = 10)]
        public string Pitch { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Draft"; // Draft, Active, At-Risk, Suspended

        public long StorageUsedBytes { get; set; } = 0;

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
        public ICollection<User> TeamMembers { get; set; } = new List<User>();
    }
}
