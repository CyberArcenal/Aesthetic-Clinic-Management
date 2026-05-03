using AestheticClinicAPI.Modules.Appointments.Models;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Modules.Photos.Models;
using AestheticClinicAPI.Modules.Reports.Models;
using AestheticClinicAPI.Modules.Staff.Models;
using AestheticClinicAPI.Modules.Treatments.Models;
using Microsoft.EntityFrameworkCore;

namespace AestheticClinicAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Clients
        public DbSet<Client> Clients { get; set; }

        // Treatments
        public DbSet<Treatment> Treatments { get; set; }

        // Staff
        public DbSet<StaffMember> StaffMembers { get; set; }

        // Appointments
        public DbSet<Appointment> Appointments { get; set; }

        // Billing
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // Notifications
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotifyLog> NotifyLogs { get; set; }

        // Photos
        public DbSet<Photo> Photos { get; set; }

        // Reports
        public DbSet<ReportLog> ReportLogs { get; set; }

        // Authentication
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Global query filters (soft delete)
            modelBuilder.Entity<Client>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Treatment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<StaffMember>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<NotificationTemplate>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<NotifyLog>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Photo>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ReportLog>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PasswordResetToken>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<UserRole>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RefreshToken>().HasQueryFilter(e => !e.IsDeleted);

            // Additional configurations (e.g., foreign keys, indexes) can be added here

            base.OnModelCreating(modelBuilder);
        }
    }
}
