using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PropertyManagement.API.Data;
using PropertyManagement.API.Models;
using PropertyManagement.MVC.ViewModels.PropertyManager;

namespace PropertyManagement.MVC.Controllers
{
    [Authorize(Roles = "PropertyManager")]
    public class PropertyManagerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PropertyManagerController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

        public IActionResult CreateBuilding() => View(new CreateBuildingViewModel());

     
        [HttpPost]
        public async Task<IActionResult> CreateBuilding(CreateBuildingViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var building = new Building
            {
                Name = model.Name,
                Address = model.Address,
                City = model.City,
                Type = model.Type
            };
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Building created successfully!";
            return RedirectToAction("Buildings");
        }

        public async Task<IActionResult> EditBuilding(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null) return NotFound();
            return View(building);
        }

        // replace the EditBuilding POST
        [HttpPost]
        public async Task<IActionResult> EditBuilding(Building building)
        {
            // the Building model has a Units list that the form doesnt send
            // so we remove it from model state otherwise IsValid is always false
            ModelState.Remove("Units");

            if (ModelState.IsValid)
            {
                var existing = await _context.Buildings.FindAsync(building.Id);
                if (existing == null) return NotFound();

                existing.Name = building.Name;
                existing.Address = building.Address;
                existing.City = building.City;
                existing.Type = building.Type;

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
            return View(new CreateUnitViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUnit(CreateUnitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Buildings = await _context.Buildings.ToListAsync();
                return View(model);
            }
            var unit = new Unit
            {
                UnitNumber = model.UnitNumber,
                Type = model.Type,
                Size = model.Size,
                Rent = model.Rent,
                Amenities = model.Amenities,
                BuildingId = model.BuildingId,
                Status = "Available"
            };
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Unit created successfully!";
            return RedirectToAction("Units");
        }

        public async Task<IActionResult> EditUnit(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();
            ViewBag.Buildings = await _context.Buildings.ToListAsync();
            return View(unit);
        }

        // replace the EditUnit POST
        [HttpPost]
        public async Task<IActionResult> EditUnit(Unit unit)
        {
            // Unit model has Building, Leases, MaintenanceRequests nav properties
            // none of them are in the form so we clear them from model state
            ModelState.Remove("Building");
            ModelState.Remove("Leases");
            ModelState.Remove("MaintenanceRequests");

            if (ModelState.IsValid)
            {
                var existing = await _context.Units.FindAsync(unit.Id);
                if (existing == null) return NotFound();

                existing.UnitNumber = unit.UnitNumber;
                existing.BuildingId = unit.BuildingId;
                existing.Type = unit.Type;
                existing.Size = unit.Size;
                existing.Rent = unit.Rent;
                existing.Amenities = unit.Amenities;

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

        public IActionResult CreateTenant() => View(new CreateTenantViewModel());

        [HttpPost]
        public async Task<IActionResult> CreateTenant(CreateTenantViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var tenant = new Tenant
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                CPR = model.CPR,
                DateRegistered = DateTime.Now
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Tenant created successfully!";
            return RedirectToAction("Tenants");
        }

        // load the tenant we want to edit based on the id from the url
        public async Task<IActionResult> EditTenant(int id)
        {
            // look it up in the database
            var tenant = await _context.Tenants.FindAsync(id);

            // if nothing was found just return a 404 page
            if (tenant == null) return NotFound();

            // pass the tenant data to the edit form
            return View(tenant);
        }

        // this runs when the user clicks Save on the edit form
        [HttpPost]
        public async Task<IActionResult> EditTenant(Tenant tenant)
        {
            // check if everything the user typed is valid
            if (ModelState.IsValid)
            {
                // tell EF this record has been changed so it updates it in the db
                _context.Entry(tenant).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Tenant updated successfully!";
                return RedirectToAction("Tenants");
            }

            // if validation failed just show the form again with the errors
            return View(tenant);
        }

        // delete a tenant by id
        [HttpPost]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);

            // only delete if we actually found something
            if (tenant != null)
            {
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tenant deleted.";
            }

            return RedirectToAction("Tenants");
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
            return View(new CreateLeaseViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateLease(CreateLeaseViewModel model)
        {
            if (model.EndDate <= model.StartDate)
                ModelState.AddModelError("EndDate", "End date must be after start date");

            if (!ModelState.IsValid)
            {
                ViewBag.Tenants = await _context.Tenants.ToListAsync();
                ViewBag.Units = await _context.Units
                    .Where(u => u.Status == "Available")
                    .Include(u => u.Building)
                    .ToListAsync();
                return View(model);
            }

            var activeLeases = await _context.Leases
                .AnyAsync(l => l.UnitId == model.UnitId && l.Status == "Active");

            if (activeLeases)
            {
                TempData["Error"] = "This unit is already occupied!";
                ViewBag.Tenants = await _context.Tenants.ToListAsync();
                ViewBag.Units = await _context.Units
                    .Where(u => u.Status == "Available")
                    .Include(u => u.Building)
                    .ToListAsync();
                return View(model);
            }

            var lease = new Lease
            {
                TenantId = model.TenantId,
                UnitId = model.UnitId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                MonthlyRent = model.MonthlyRent,
                Status = "Application"
            };
            _context.Leases.Add(lease);

            var unit = await _context.Units.FindAsync(model.UnitId);
            if (unit != null) unit.Status = "Occupied";

            await _context.SaveChangesAsync();
            TempData["Success"] = "Lease created successfully!";
            return RedirectToAction("Leases");
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

        public IActionResult CreateStaff() => View(new CreateStaffViewModel());

        [HttpPost]
        public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var staff = new MaintenanceStaff
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                SkillType = model.SkillType,
                AvailabilityStatus = "Available"
            };
            _context.MaintenanceStaffs.Add(staff);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Staff member added successfully!";
            return RedirectToAction("Staff");
        }
        // load the staff member we want to edit
        public async Task<IActionResult> EditStaff(int id)
        {
            var staff = await _context.MaintenanceStaffs.FindAsync(id);

            if (staff == null) return NotFound();

            return View(staff);
        }

        // save the changes from the edit form
        [HttpPost]
        public async Task<IActionResult> EditStaff(MaintenanceStaff staff)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(staff).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Staff member updated successfully!";
                return RedirectToAction("Staff");
            }

            return View(staff);
        }

        // delete a staff member
        [HttpPost]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var staff = await _context.MaintenanceStaffs.FindAsync(id);

            if (staff != null)
            {
                _context.MaintenanceStaffs.Remove(staff);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Staff member deleted.";
            }

            return RedirectToAction("Staff");
        }
        public IActionResult MaintenanceBoard()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
            return View();
        }
    }
}
