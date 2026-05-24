using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.Models;

namespace PropertyManagement.MVC.Controllers
{
    [Authorize(Roles = "Tenant")]
    public class TenantController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TenantController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email == user!.Email);

            if (tenant == null) return View("NoProfile");

            ViewBag.ActiveLease = await _context.Leases
                .Include(l => l.Unit).ThenInclude(u => u.Building)
                .FirstOrDefaultAsync(l => l.TenantId == tenant.Id && l.Status == "Active");

            ViewBag.PendingRequests = await _context.MaintenanceRequests
                .CountAsync(m => m.TenantId == tenant.Id && m.Status != "Closed");

            ViewBag.Notifications = await _context.Notifications
                .Where(n => n.TenantId == tenant.Id && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(tenant);
        }

        public async Task<IActionResult> MyLease()
        {
            var user = await _userManager.GetUserAsync(User);
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email == user!.Email);

            if (tenant == null) return View("NoProfile");

            var leases = await _context.Leases
                .Include(l => l.Unit).ThenInclude(u => u.Building)
                .Include(l => l.Payments)
                .Where(l => l.TenantId == tenant.Id)
                .ToListAsync();

            return View(leases);
        }

        public async Task<IActionResult> MaintenanceRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email == user!.Email);

            if (tenant == null) return View("NoProfile");

            var requests = await _context.MaintenanceRequests
                .Include(m => m.Unit)
                .Include(m => m.MaintenanceStaff)
                .Where(m => m.TenantId == tenant.Id)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> CreateMaintenanceRequest()
        {
            var user = await _userManager.GetUserAsync(User);
            var tenant = await _context.Tenants
                .Include(t => t.Leases).ThenInclude(l => l.Unit)
                .FirstOrDefaultAsync(t => t.Email == user!.Email);

            if (tenant == null) return View("NoProfile");

            ViewBag.Units = tenant.Leases
                .Where(l => l.Status == "Active")
                .Select(l => l.Unit)
                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMaintenanceRequest(MaintenanceRequest request)
        {
            // Remove nav-prop entries that the form never posts so IsValid works correctly
            ModelState.Remove("Tenant");
            ModelState.Remove("Unit");
            ModelState.Remove("MaintenanceStaff");

            var user = await _userManager.GetUserAsync(User);
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email == user!.Email);

            if (tenant == null) return View("NoProfile");

            if (!ModelState.IsValid)
            {
                ViewBag.Units = tenant.Leases
                    .Where(l => l.Status == "Active")
                    .Select(l => l.Unit)
                    .ToList();
                return View(request);
            }

            request.TenantId     = tenant.Id;
            request.TicketNumber = "TKT" + DateTime.Now.Ticks.ToString().Substring(0, 8);
            request.CreatedAt    = DateTime.Now;
            request.Status       = "Submitted";

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Request submitted! Ticket: {request.TicketNumber}";
            return RedirectToAction("MaintenanceRequests");
        }
        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email == user!.Email);

            if (tenant != null)
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenant.Id);

                if (notification != null)
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Dashboard");
        }

    }
}