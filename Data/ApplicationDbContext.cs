using AuthorizationForm.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationForm.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AuthorizationRequest> AuthorizationRequests { get; set; }
        public DbSet<RequestHistory> RequestHistories { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ApplicationSystem> Systems { get; set; }
        public DbSet<FormTemplate> FormTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Manager)
                .WithMany()
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuthorizationRequest>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuthorizationRequest>()
                .HasOne(r => r.Manager)
                .WithMany()
                .HasForeignKey(r => r.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuthorizationRequest>()
                .HasOne(r => r.FinalApprover)
                .WithMany()
                .HasForeignKey(r => r.FinalApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuthorizationRequest>()
                .HasOne(r => r.ChangedByAdmin)
                .WithMany()
                .HasForeignKey(r => r.ChangedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RequestHistory>()
                .HasOne(h => h.Request)
                .WithMany(r => r.History)
                .HasForeignKey(h => h.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FormTemplate>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed default systems
            builder.Entity<ApplicationSystem>().HasData(
                new ApplicationSystem { Id = 1, Name = "מערכת HR", Description = "מערכת ניהול משאבי אנוש", Category = "ניהול", IsActive = true },
                new ApplicationSystem { Id = 2, Name = "מערכת כספים", Description = "מערכת ניהול כספים", Category = "כספים", IsActive = true },
                new ApplicationSystem { Id = 3, Name = "מערכת מכירות", Description = "מערכת ניהול מכירות", Category = "מכירות", IsActive = true },
                new ApplicationSystem { Id = 4, Name = "מערכת לוגיסטיקה", Description = "מערכת ניהול לוגיסטיקה", Category = "לוגיסטיקה", IsActive = true }
            );
        }
    }
}

