using Microsoft.EntityFrameworkCore;
using SCIM_Application.Models;

namespace SCIM_Application.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<UserApplication> UserApplications => Set<UserApplication>();
        public DbSet<ScimLog> ScimLogs => Set<ScimLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(x => x.UserName).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.ScimId).IsUnique();
            });

            modelBuilder.Entity<UserApplication>(e =>
            {
                e.HasIndex(x => new { x.UserId, x.ApplicationId }).IsUnique();
                e.HasOne(x => x.User).WithMany(u => u.UserApplications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Application).WithMany(a => a.UserApplications).HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
            });

            // ScimLog için cascade delete davranışını tanımla
            modelBuilder.Entity<ScimLog>(e =>
            {
                e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(x => x.Application).WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
