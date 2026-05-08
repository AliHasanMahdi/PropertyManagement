using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.DTOs.Maintenance;
using PropertyManagement.API.Hubs;
using PropertyManagement.API.Models;
using PropertyManagement.API.Services.Interfaces;

namespace PropertyManagement.API.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<MaintenanceHub> _hubContext;

        public MaintenanceService(AppDbContext context, IHubContext<MaintenanceHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<MaintenanceResponseDto>> GetAllAsync()
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.MaintenanceStaff)
                .Select(m => MapToDto(m))
                .ToListAsync();
        }

        public async Task<MaintenanceResponseDto?> GetByIdAsync(int id)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.MaintenanceStaff)
                .FirstOrDefaultAsync(m => m.Id == id);

            return request == null ? null : MapToDto(request);
        }

        public async Task<MaintenanceResponseDto?> LookupAsync(string ticketNumber, string phone)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.MaintenanceStaff)
                .FirstOrDefaultAsync(m =>
                    m.TicketNumber == ticketNumber &&
                    m.Tenant.Phone == phone);

            return request == null ? null : MapToDto(request);
        }

        public async Task<MaintenanceResponseDto> CreateAsync(CreateMaintenanceRequestDto dto)
        {
            var request = new MaintenanceRequest
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Priority = dto.Priority,
                UnitId = dto.UnitId,
                TenantId = dto.TenantId,
                TicketNumber = "TKT" + DateTime.Now.Ticks.ToString()[..8],
                CreatedAt = DateTime.Now,
                Status = "Submitted"
            };

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            await _context.Entry(request).Reference(r => r.Tenant).LoadAsync();
            await _context.Entry(request).Reference(r => r.Unit).LoadAsync();

            var dto2 = MapToDto(request);

            // Broadcast new request to all connected clients
            await _hubContext.Clients.Group("MaintenanceBoard")
                .SendAsync("NewRequest", dto2);

            return dto2;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var validStatuses = new[]
            { "Submitted", "Assigned", "InProgress", "Resolved", "Closed" };

            if (!validStatuses.Contains(status)) return false;

            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.MaintenanceStaff)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return false;

            request.Status = status;
            if (status == "Resolved") request.ResolvedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Broadcast status update to all connected clients
            await _hubContext.Clients.Group("MaintenanceBoard")
                .SendAsync("StatusUpdated", MapToDto(request));

            return true;
        }

        public async Task<bool> AssignStaffAsync(int id, int staffId)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.MaintenanceStaff)
                .FirstOrDefaultAsync(m => m.Id == id);

            var staff = await _context.MaintenanceStaffs.FindAsync(staffId);

            if (request == null || staff == null) return false;

            request.MaintenanceStaffId = staffId;
            request.Status = "Assigned";
            staff.AvailabilityStatus = "Busy";

            var notification = new Notification
            {
                MaintenanceStaffId = staffId,
                Message = $"You have been assigned to request #{request.TicketNumber}",
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            // Broadcast assignment to all connected clients
            await _hubContext.Clients.Group("MaintenanceBoard")
                .SendAsync("StaffAssigned", MapToDto(request));

            return true;
        }

        private static MaintenanceResponseDto MapToDto(MaintenanceRequest m) => new()
        {
            Id = m.Id,
            TicketNumber = m.TicketNumber,
            Title = m.Title,
            Description = m.Description,
            Category = m.Category,
            Priority = m.Priority,
            Status = m.Status,
            CreatedAt = m.CreatedAt,
            ResolvedAt = m.ResolvedAt,
            TenantName = m.Tenant?.FullName ?? "",
            UnitNumber = m.Unit?.UnitNumber ?? "",
            AssignedStaffName = m.MaintenanceStaff?.FullName
        };
    }
}