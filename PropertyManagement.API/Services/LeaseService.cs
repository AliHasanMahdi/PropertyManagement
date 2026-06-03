using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.DTOs.Lease;
using PropertyManagement.API.Models;
using PropertyManagement.API.Services.Interfaces;

namespace PropertyManagement.API.Services
{
    public class LeaseService : ILeaseService
    {
        private readonly AppDbContext _context;

        public LeaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaseResponseDto>> GetAllAsync()
        {
            return await _context.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Unit).ThenInclude(u => u.Building)
                .Include(l => l.Payments)
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        public async Task<LeaseResponseDto?> GetByIdAsync(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Unit).ThenInclude(u => u.Building)
                .Include(l => l.Payments)
                .FirstOrDefaultAsync(l => l.Id == id);

            return lease == null ? null : MapToDto(lease);
        }

        public async Task<(bool Success, string Message, LeaseResponseDto? Lease)> CreateAsync(CreateLeaseDto dto)
        {
            // Validate dates
            if (dto.EndDate <= dto.StartDate)
                return (false, "End date must be after start date", null);

            // Check unit availability
            var isOccupied = await _context.Leases
                .AnyAsync(l => l.UnitId == dto.UnitId && l.Status == "Active");

            if (isOccupied)
                return (false, "This unit is already occupied!", null);

            var lease = new Lease
            {
                TenantId = dto.TenantId,
                UnitId = dto.UnitId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MonthlyRent = dto.MonthlyRent,
                Status = "Application"
            };

            _context.Leases.Add(lease);

            var unit = await _context.Units.FindAsync(dto.UnitId);
            if (unit != null) unit.Status = "Occupied";

            // Notify tenant
            var notification = new Notification
            {
                TenantId = dto.TenantId,
                Message = "Your lease application has been submitted successfully.",
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            await _context.Entry(lease).Reference(l => l.Tenant).LoadAsync();
            await _context.Entry(lease).Reference(l => l.Unit).Query()
                .Include(u => u.Building).LoadAsync();

            return (true, "Lease created successfully", MapToDto(lease));
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var validStatuses = new[] { "Application", "Screening", "Active", "Renewal", "Terminated" };
            if (!validStatuses.Contains(status)) return false;

            var lease = await _context.Leases
                .Include(l => l.Unit)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null) return false;

            lease.Status = status;

            if (status == "Terminated" && lease.Unit != null)
                lease.Unit.Status = "Available";

            var notification = new Notification
            {
                TenantId = lease.TenantId,
                Message = $"Your lease status has been updated to: {status}",
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddPaymentAsync(int leaseId, AddPaymentDto dto)
        {
            var lease = await _context.Leases.FindAsync(leaseId);
            if (lease == null) return false;

            var payment = new Payment
            {
                LeaseId = leaseId,
                Amount = dto.Amount,
                PaymentDate = DateTime.Now,
                // Use Paid status for payments created via this flow
                Status = "Paid",
                Notes = dto.Notes
            };

            // If an optional DueDate was supplied, set it (keeps compatibility)
            if (dto.DueDate.HasValue)
                payment.DueDate = dto.DueDate;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }

            private static LeaseResponseDto MapToDto(Lease l) => new()
        {
            Id = l.Id,
            Status = l.Status,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            MonthlyRent = l.MonthlyRent,
            TenantName = l.Tenant?.FullName ?? "",
            UnitNumber = l.Unit?.UnitNumber ?? "",
            BuildingName = l.Unit?.Building?.Name ?? "",
            TotalPaid = l.Payments?.Where(p => p.Status == "Paid").Sum(p => p.Amount) ?? 0,
            // Overdue logic: payment not Paid AND DueDate < today
            HasOverduePayments = l.Payments?.Any(p => p.Status != "Paid" && p.DueDate.HasValue && p.DueDate.Value.Date < DateTime.Today) ?? false
        };
    }
}