using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using PropertyManagement.API.Data;

namespace PropertyManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceRequestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaintenanceRequestController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/maintenancerequest/track/TKT-1001/555-0199
        [HttpGet("track/{ticketNumber}/{phone}")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackRequest(string ticketNumber, string phone)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber) || string.IsNullOrWhiteSpace(phone))
            {
                return BadRequest(new { message = "Parameters missing." });
            }

            var request = await _context.MaintenanceRequests
                .Include(m => m.Unit)
                    .ThenInclude(u => u.Building)
                .Include(m => m.Tenant)
                .Include(m => m.MaintenanceStaff)
                .FirstOrDefaultAsync(m => m.TicketNumber == ticketNumber.Trim() && m.Tenant.Phone == phone.Trim());

            if (request == null)
            {
                return NotFound(new { message = "Record not found." });
            }

            return Ok(new
            {
                TicketNumber = request.TicketNumber,
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                Priority = request.Priority,
                Status = request.Status,
                CreatedAt = request.CreatedAt.ToString("dd-MMM-yy"),
                AssignedStaff = request.MaintenanceStaff != null ? request.MaintenanceStaff.FullName : "Not Assigned Yet",
                BuildingName = request.Unit?.Building?.Name ?? "Unassigned Location",
                UnitNumber = request.Unit?.UnitNumber ?? "N/A"
            });
        }
    }
}