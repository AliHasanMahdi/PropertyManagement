using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;

namespace PropertyManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "PropertyManager")]
    public class ReportingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("occupancy")]
        public async Task<IActionResult> GetOccupancy()
        {
            var total = await _context.Units.CountAsync();
            var occupied = await _context.Units.CountAsync(u => u.Status == "Occupied");
            var available = total - occupied;

            return Ok(new
            {
                TotalUnits = total,
                OccupiedUnits = occupied,
                AvailableUnits = available,
                OccupancyRate = total > 0 ? (double)occupied / total * 100 : 0
            });
        }

        [HttpGet("maintenance-stats")]
        public async Task<IActionResult> GetMaintenanceStats()
        {
            var total = await _context.MaintenanceRequests.CountAsync();
            var resolved = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Resolved");
            var pending = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Submitted");

            return Ok(new
            {
                TotalRequests = total,
                ResolvedRequests = resolved,
                PendingRequests = pending
            });
        }

        [HttpGet("overdue-payments")]
        public async Task<IActionResult> GetOverduePayments()
        {
            var overdue = await _context.Payments
                .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
                .Where(p => p.Status == "Overdue")
                .ToListAsync();

            return Ok(overdue);
        }
    }
}