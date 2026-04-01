using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Models;

namespace PropertyManagement.API.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Building> Buildings { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Lease> Leases { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<MaintenanceStaff> MaintenanceStaffs { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Building → Units
            modelBuilder.Entity<Unit>()
                .HasOne(u => u.Building)
                .WithMany(b => b.Units)
                .HasForeignKey(u => u.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lease → Tenant
            modelBuilder.Entity<Lease>()
                .HasOne(l => l.Tenant)
                .WithMany(t => t.Leases)
                .HasForeignKey(l => l.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lease → Unit
            modelBuilder.Entity<Lease>()
                .HasOne(l => l.Unit)
                .WithMany(u => u.Leases)
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // MaintenanceRequest → Tenant
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Tenant)
                .WithMany(t => t.MaintenanceRequests)
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // MaintenanceRequest → Unit
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Unit)
                .WithMany(u => u.MaintenanceRequests)
                .HasForeignKey(m => m.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // MaintenanceRequest → MaintenanceStaff
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.MaintenanceStaff)
                .WithMany(s => s.MaintenanceRequests)
                .HasForeignKey(m => m.MaintenanceStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // Payment → Lease
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Lease)
                .WithMany(l => l.Payments)
                .HasForeignKey(p => p.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification → Tenant
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Tenant)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Notification → MaintenanceStaff
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.MaintenanceStaff)
                .WithMany(s => s.Notifications)
                .HasForeignKey(n => n.MaintenanceStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // Decimal precision
            modelBuilder.Entity<Unit>()
                .Property(u => u.Rent)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Lease>()
                .Property(l => l.MonthlyRent)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
