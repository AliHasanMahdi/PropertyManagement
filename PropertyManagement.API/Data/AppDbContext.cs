using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Models;
using System;

namespace PropertyManagement.API.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>, IDataProtectionKeyContext
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

        // Required by IDataProtectionKeyContext - stores auth key ring in the database
        // so cookies survive Azure App Service restarts and scale-out
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity configuration mappings must evaluate first
            base.OnModelCreating(modelBuilder);

            // =================================================================
            // 1. DOMAIN RELATIONSHIPS & CONSTRAINTS
            // =================================================================

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

            // Column Types & Precision assignments
            modelBuilder.Entity<Unit>()
                .Property(u => u.Rent)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Lease>()
                .Property(l => l.MonthlyRent)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            // =================================================================
            // 2. MODEL-ALIGNED SEED DATA
            // =================================================================

            // A. Seed Buildings (Matched with Name, Address, City, Type)
            modelBuilder.Entity<Building>().HasData(
                new Building { Id = 1, Name = "Grandview Heights", Address = "101 Luxury Way", City = "Manama", Type = "Residential" },
                new Building { Id = 2, Name = "Maple Wood Tower", Address = "202 Timber Lane", City = "Seef", Type = "Commercial" }
            );

            // B. Seed Units (UnitNumber, Type, Size, Rent, Amenities, Status)
            modelBuilder.Entity<Unit>().HasData(
                new Unit { Id = 1, BuildingId = 1, UnitNumber = "101A", Type = "Apartment", Size = 85.5, Rent = 1200.00M, Amenities = "Balcony, AC", Status = "Occupied" },
                new Unit { Id = 2, BuildingId = 1, UnitNumber = "102B", Type = "Studio", Size = 45.0, Rent = 1350.00M, Amenities = "Furnished", Status = "Available" },
                new Unit { Id = 3, BuildingId = 2, UnitNumber = "201", Type = "Office", Size = 120.0, Rent = 2450.00M, Amenities = "Conference Room", Status = "Occupied" }
            );

            // C. Seed Tenants (FullName, Email, Phone, CPR, DateRegistered)
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant { Id = 1, FullName = "John Doe", Email = "tenant1@example.com", Phone = "555-0199", CPR = "990112345", DateRegistered = new DateTime(2026, 1, 1) },
                new Tenant { Id = 2, FullName = "Jane Smith", Email = "tenant2@example.com", Phone = "555-0144", CPR = "950554321", DateRegistered = new DateTime(2026, 2, 1) }
            );

            // D. Seed Leases (StartDate, EndDate, MonthlyRent, Status)
            modelBuilder.Entity<Lease>().HasData(
                new Lease { Id = 1, UnitId = 1, TenantId = 1, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), MonthlyRent = 1200.00M, Status = "Active" },
                new Lease { Id = 2, UnitId = 3, TenantId = 2, StartDate = new DateTime(2026, 2, 1), EndDate = new DateTime(2027, 1, 31), MonthlyRent = 2450.00M, Status = "Active" }
            );

            // E. Seed Maintenance Staff Profiles
            modelBuilder.Entity<MaintenanceStaff>().HasData(
                new MaintenanceStaff { Id = 1, FullName = "Bob Builder", Email = "staff@property.com", Phone = "555-0122", SkillType = "Plumbing", AvailabilityStatus = "Available" }
            );

            // F. Seed Maintenance Requests (TicketNumber, Title, Description, Category, Priority, Status, CreatedAt)
            modelBuilder.Entity<MaintenanceRequest>().HasData(
                new MaintenanceRequest { Id = 1, TicketNumber = "TKT-1001", Title = "Leaky Kitchen Sink", Description = "The pipe below the kitchen sink is constantly dripping water onto the cabinet base.", Category = "Plumbing", Priority = "High", Status = "Assigned", UnitId = 1, TenantId = 1, MaintenanceStaffId = 1, CreatedAt = new DateTime(2026, 5, 15) },
                new MaintenanceRequest { Id = 2, TicketNumber = "TKT-1002", Title = "Broken Light Switch", Description = "The bedroom toggle switch clicks but the light fixtures do not respond.", Category = "Electrical", Priority = "Medium", Status = "InProgress", UnitId = 3, TenantId = 2, MaintenanceStaffId = 1, CreatedAt = new DateTime(2026, 5, 16) }
            );

            // G. Seed Payments (Amount, PaymentDate, Status, Notes)
            modelBuilder.Entity<Payment>().HasData(
                new Payment { Id = 1, LeaseId = 1, Amount = 1200.00M, PaymentDate = new DateTime(2026, 5, 1), Status = "Paid", Notes = "Rent payment for May" },
                new Payment { Id = 2, LeaseId = 2, Amount = 2450.00M, PaymentDate = new DateTime(2026, 5, 2), Status = "Paid", Notes = "First month rent deposit" }
            );
        }
    }
}