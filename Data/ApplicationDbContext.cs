using AgencyFlow.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgencyFlow.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }
        public DbSet<Earning> Earnings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fluent API Configurations

            // 1-to-1: User <-> UserProfile
            builder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-to-1: WorkLog <-> Earning
            builder.Entity<WorkLog>()
                .HasOne(w => w.Earning)
                .WithOne(e => e.WorkLog)
                .HasForeignKey<Earning>(e => e.WorkLogId)
                .OnDelete(DeleteBehavior.Cascade);

            // EventFee precision configuration
            builder.Entity<Event>()
                .Property(e => e.EventFee)
                .HasColumnType("decimal(18,2)");

            // Decimal configuration for UserProfile
            builder.Entity<UserProfile>()
                .Property(u => u.HourlyRate)
                .HasColumnType("decimal(18,2)");

            // Decimal configuration for WorkLog
            builder.Entity<WorkLog>()
                .Property(w => w.ApprovedHours)
                .HasColumnType("decimal(18,2)");

            // Decimal configurations for Earning
            builder.Entity<Earning>()
                .Property(e => e.TotalAmount)
                .HasColumnType("decimal(18,2)");
                
            builder.Entity<Earning>()
                .Property(e => e.AppliedHourlyRate)
                .HasColumnType("decimal(18,2)");
                
            // Handle Assignment Many-to-Many equivalent
            builder.Entity<Assignment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Assignments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Assignment>()
                .HasOne(a => a.Event)
                .WithMany(e => e.Assignments)
                .HasForeignKey(a => a.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Assignment <-> WorkLog configuration
            builder.Entity<WorkLog>()
                .HasOne(w => w.Assignment)
                .WithMany(a => a.WorkLogs)
                .HasForeignKey(w => w.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
