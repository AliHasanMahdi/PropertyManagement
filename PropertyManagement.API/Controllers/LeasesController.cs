using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.Models;

namespace PropertyManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeasesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeasesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> GetAll()
        {
            var leases = await _context.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Unit)
                .ThenInclude(u => u.Building)
                .Include(l => l.Payments)
                .ToListAsync();
            return Ok(leases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Unit)
                .ThenInclude(u => u.Building)
                .Include(l => l.Payments)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lease == null) return NotFound();
            return Ok(lease);
        }

        [HttpPost]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Create([FromBody] Lease lease)
        {
            // Check if unit is already occupied
            var activeLeases = await _context.Leases
                .AnyAsync(l => l.UnitId == lease.UnitId && l.Status == "Active");
            if (activeLeases)
                return BadRequest(new { message = "Unit is already occupied!" });

            lease.Status = "Application";
            _context.Leases.Add(lease);

            // Update unit status
            var unit = await _context.Units.FindAsync(lease.UnitId);
            if (unit != null) unit.Status = "Occupied";

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = lease.Id }, lease);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateLeaseStatusDto dto)
        {
            var lease = await _context.Leases
                .Include(l => l.Unit)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lease == null) return NotFound();

            lease.Status = dto.Status;

            // If terminated, set unit back to available
            if (dto.Status == "Terminated" && lease.Unit != null)
                lease.Unit.Status = "Available";

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/payments")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> AddPayment(int id, [FromBody] Payment payment)
        {
            var lease = await _context.Leases.FindAsync(id);
            if (lease == null) return NotFound();

            payment.LeaseId = id;
            payment.PaymentDate = DateTime.Now;
            payment.Status = "Paid";
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return Ok(payment);
        }
    }

    public class UpdateLeaseStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
