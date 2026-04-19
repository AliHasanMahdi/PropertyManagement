using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.Models;

namespace PropertyManagement.MVC.Controllers
{
    [Authorize(Roles = "PropertyManager")]
    public class PropertyManagerController : Controller
    {
        private readonly AppDbContext _context;

        public PropertyManagerController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== DASHBOARD ====================
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalBuildings = await _context.Buildings.CountAsync();
            ViewBag.TotalUnits = await _context.Units.CountAsync();
            ViewBag.OccupiedUnits = await _context.Units.CountAsync(u => u.Status == "Occupied");
            ViewBag.AvailableUnits = await _context.Units.CountAsync(u => u.Status == "Available");
            ViewBag.TotalTenants = await _context.Tenants.CountAsync();
            ViewBag.PendingRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Submitted");
            ViewBag.ActiveLeases = await _context.Leases.CountAsync(l => l.Status == "Active");
            ViewBag.OverduePayments = await _context.Payments.CountAsync(p => p.Status == "Overdue");
            return View();
        }

        // ==================== BUILDINGS ====================
        public async Task<IActionResult> Buildings()
        {
            var buildings = await _context.Buildings
                .Include(b => b.Units)
                .ToListAsync();
            return View(buildings);
        }

        public IActionResult CreateBuilding() => View();

        [HttpPost]
        public async Task<IActionResult> CreateBuilding(Building building)
        {
            if (ModelState.IsValid)
            {
                _context.Buildings.Add(building);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Building created successfully!";
                return RedirectToAction("Buildings");
            }
            return View(building);
        }

        public async Task<IActionResult> EditBuilding(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null) return NotFound();
            return View(building);
        }

        [HttpPost]
        public async Task<IActionResult> EditBuilding(Building building)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(building).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Building updated successfully!";
                return RedirectToAction("Buildings");
            }
            return View(building);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building != null)
            {
                _context.Buildings.Remove(building);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Building deleted successfully!";
            }
            return RedirectToAction("Buildings");
        }

        // ==================== UNITS ====================
        public async Task<IActionResult> Units()
        {
            var units = await _context.Units
                .Include(u => u.Building)
                .ToListAsync();
            return View(units);
        }

        public async Task<IActionResult> CreateUnit()
        {
            ViewBag.Buildings = await _context.Buildings.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUnit(Unit unit)
        {
            if (ModelState.IsValid)
            {
                _context.Units.Add(unit);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Unit created successfully!";
                return RedirectToAction("Units");
            }
            ViewBag.Buildings = await _context.Buildings.ToListAsync();
            return View(unit);
        }

        public async Task<IActionResult> EditUnit(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();
            ViewBag.Buildings = await _context.Buildings.ToListAsync();
            return View(unit);
        }

        [HttpPost]
        public async Task<IActionResult> EditUnit(Unit unit)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(unit).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Unit updated successfully!";
                return RedirectToAction("Units");
            }
            ViewBag.Buildings = await _context.Buildings.ToListAsync();
            return View(unit);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit != null)
            {
                _context.Units.Remove(unit);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Unit deleted successfully!";
            }
            return RedirectToAction("Units");
        }

        // ==================== TENANTS ====================
        public async Task<IActionResult> Tenants()
        {
            var tenants = await _context.Tenants
                .Include(t => t.Leases)
                .ToListAsync();
            return View(tenants);
        }

        public IActionResult CreateTenant() => View();

        [HttpPost]
        public async Task<IActionResult> CreateTenant(Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                tenant.DateRegistered = DateTime.Now;
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tenant created successfully!";
                return RedirectToAction("Tenants");
            }
            return View(tenant);
        }

        // ==================== LEASES ====================
        public async Task<IActionResult> Leases()
        {
            var leases = await _context.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Unit)
                .ThenInclude(u => u.Building)
                .Include(l => l.Payments)
                .ToListAsync();
            return View(leases);
        }

        public async Task<IActionResult> CreateLease()
        {
            ViewBag.Tenants = await _context.Tenants.ToListAsync();
            ViewBag.Units = await _context.Units
                .Where(u => u.Status == "Available")
                .Include(u => u.Building)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateLease(Lease lease)
        {
            // Check if unit is already occupied
            var activeLeases = await _context.Leases
                .AnyAsync(l => l.UnitId == lease.UnitId && l.Status == "Active");

            if (activeLeases)
            {
                TempData["Error"] = "This unit is already occupied!";
                ViewBag.Tenants = await _context.Tenants.ToListAsync();
                ViewBag.Units = await _context.Units
                    .Where(u => u.Status == "Available")
                    .Include(u => u.Building)
                    .ToListAsync();
                return View(lease);
            }

            if (ModelState.IsValid)
            {
                lease.Status = "Application";
                _context.Leases.Add(lease);

                var unit = await _context.Units.FindAsync(lease.UnitId);
                if (unit != null) unit.Status = "Occupied";

                await _context.SaveChangesAsync();
                TempData["Success"] = "Lease created successfully!";
                return RedirectToAction("Leases");
            }

            ViewBag.Tenants = await _context.Tenants.ToListAsync();
            ViewBag.Units = await _context.Units
                .Where(u => u.Status == "Available")
                .Include(u => u.Building)
                .ToListAsync();
            return View(lease);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLeaseStatus(int id, string status)
        {
            var lease = await _context.Leases
                .Include(l => l.Unit)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease != null)
            {
                lease.Status = status;
                if (status == "Terminated" && lease.Unit != null)
                    lease.Unit.Status = "Available";

                await _context.SaveChangesAsync();

                // Send notification to tenant
                var notification = new Notification
                {
                    TenantId = lease.TenantId,
                    Message = $"Your lease status has been updated to: {status}",
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Lease status updated!";
            }
            return RedirectToAction("Leases");
        }

        // ==================== MAINTENANCE ====================
        public async Task<IActionResult> MaintenanceRequests()
        {
            var requests = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .ThenInclude(u => u.Building)
                .Include(m => m.MaintenanceStaff)
                .ToListAsync();
            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> AssignStaff(int requestId, int staffId)
        {
            var request = await _context.MaintenanceRequests.FindAsync(requestId);
            var staff = await _context.MaintenanceStaffs.FindAsync(staffId);

            if (request != null && staff != null)
            {
                request.MaintenanceStaffId = staffId;
                request.Status = "Assigned";
                staff.AvailabilityStatus = "Busy";

                // Notify staff
                var notification = new Notification
                {
                    MaintenanceStaffId = staffId,
                    Message = $"You have been assigned to maintenance request #{request.TicketNumber}",
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Staff assigned successfully!";
            }
            return RedirectToAction("MaintenanceRequests");
        }

        // ==================== PAYMENTS ====================
        public async Task<IActionResult> Payments()
        {
            var payments = await _context.Payments
                .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
                .ToListAsync();
            return View(payments);
        }

        [HttpPost]
        public async Task<IActionResult> AddPayment(int leaseId, decimal amount, string notes)
        {
            var payment = new Payment
            {
                LeaseId = leaseId,
                Amount = amount,
                PaymentDate = DateTime.Now,
                Status = "Paid",
                Notes = notes
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Payment recorded successfully!";
            return RedirectToAction("Payments");
        }

        // ==================== STAFF ====================
        public async Task<IActionResult> Staff()
        {
            var staff = await _context.MaintenanceStaffs.ToListAsync();
            return View(staff);
        }

        public IActionResult CreateStaff() => View();

        [HttpPost]
        public async Task<IActionResult> CreateStaff(MaintenanceStaff staff)
        {
            if (ModelState.IsValid)
            {
                _context.MaintenanceStaffs.Add(staff);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Staff member created successfully!";
                return RedirectToAction("Staff");
            }
            return View(staff);
        }
    }
}
