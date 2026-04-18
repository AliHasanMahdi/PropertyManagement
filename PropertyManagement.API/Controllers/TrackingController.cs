using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PropertyManagement.API.Data;

namespace PropertyManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TrackingController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TrackingController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("{reference}")]
        public async Task<IActionResult> Get(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
                return BadRequest(new { message = "reference is required" });

            if (int.TryParse(reference, out var leaseId))
            {
                var lease = await _db.Leases
                    .Include(l => l.Tenant)
                    .Include(l => l.Unit).ThenInclude(u => u.Building)
                    .Include(l => l.Payments)
                    .FirstOrDefaultAsync(l => l.Id == leaseId);

                if (lease == null) return NotFound();

                return Ok(new
                {
                    type = "lease",
                    leaseId = lease.Id,
                    StartDate = lease.StartDate,
                    EndDate = lease.EndDate,
                    MonthlyRent = lease.MonthlyRent,
                    Status = lease.Status,
                    tenant = new
                    {
                        Id = lease.Tenant.Id,
                        FullName = lease.Tenant.FullName,
                        Email = lease.Tenant.Email,
                        CPR = lease.Tenant.CPR
                    },
                    unit = new
                    {
                        Id = lease.Unit.Id,
                        UnitNumber = lease.Unit.UnitNumber,
                        Rent = lease.Unit.Rent,
                        building = new
                        {
                            Id = lease.Unit.Building.Id,
                            Name = lease.Unit.Building.Name,
                            Address = lease.Unit.Building.Address
                        }
                    },
                    payments = lease.Payments.Select(p => new
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        Status = p.Status
                    })
                });
            }

            var tenant = await _db.Tenants
                .Include(t => t.Leases).ThenInclude(l => l.Unit).ThenInclude(u => u.Building)
                .Include(t => t.Notifications)
                .FirstOrDefaultAsync(t => t.CPR == reference || t.Email == reference);

            if (tenant != null)
            {
                var latestLease = tenant.Leases.OrderByDescending(l => l.StartDate).FirstOrDefault();
                return Ok(new
                {
                    type = "tenant",
                    tenant = new
                    {
                        Id = tenant.Id,
                        FullName = tenant.FullName,
                        Email = tenant.Email,
                        CPR = tenant.CPR,
                        DateRegistered = tenant.DateRegistered
                    },
                    latestLease = latestLease == null ? null : new
                    {
                        Id = latestLease.Id,
                        StartDate = latestLease.StartDate,
                        EndDate = latestLease.EndDate,
                        MonthlyRent = latestLease.MonthlyRent,
                        Status = latestLease.Status,
                        unit = latestLease.Unit == null ? null : new
                        {
                            Id = latestLease.Unit.Id,
                            UnitNumber = latestLease.Unit.UnitNumber,
                            Building = new { Id = latestLease.Unit.Building.Id, Name = latestLease.Unit.Building.Name }
                        }
                    },
                    notifications = tenant.Notifications.Select(n => new { n.Id, n.Message, CreatedAt = n.CreatedAt })
                });
            }

            return NotFound();
        }
    }
}