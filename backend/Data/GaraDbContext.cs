using GaraShowcase.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GaraShowcase.Api.Data
{
    public class GaraDbContext : DbContext
    {
        public GaraDbContext(DbContextOptions<GaraDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Milestone> Milestones { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<Application> Applications { get; set; } = null!;
        public DbSet<ApplicationLockout> ApplicationLockouts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Project Unique Constraints
            modelBuilder.Entity<Project>()
                .HasIndex(p => p.Name)
                .IsUnique();

            // Index for dormancy scans (BR-08 / BR-09)
            modelBuilder.Entity<Project>()
                .HasIndex(p => new { p.LastUpdatedAt, p.Status });

            // Index for application limits checking (BR-05)
            modelBuilder.Entity<Application>()
                .HasIndex(a => new { a.StudentId, a.Status });

            // Index for lockouts checking (BR-06)
            modelBuilder.Entity<ApplicationLockout>()
                .HasIndex(l => new { l.StudentId, l.JobId });

            // Project -> Milestones relationship (Cascades delete)
            modelBuilder.Entity<Milestone>()
                .HasOne(m => m.Project)
                .WithMany(p => p.Milestones)
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> Jobs relationship (Cascades delete)
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Project)
                .WithMany(p => p.Jobs)
                .HasForeignKey(j => j.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> TeamMembers relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Project)
                .WithMany(p => p.TeamMembers)
                .HasForeignKey(u => u.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
