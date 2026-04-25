using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.Models;

namespace PropertyManagement.MVC.Controllers
{
    [Authorize(Roles = "MaintenanceStaff")]
    public class MaintenanceStaffController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MaintenanceStaffController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Lists maintenance requests assigned to the currently logged-in maintenance staff
        public async Task<IActionResult> AssignedRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return NotFound("Maintenance staff profile not found.");

            var requests = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Where(m => m.MaintenanceStaffId == staff.Id)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // Details of a single assigned request
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return NotFound("Maintenance staff profile not found.");

            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.MaintenanceStaff)
                .FirstOrDefaultAsync(m => m.Id == id && m.MaintenanceStaffId == staff.Id);

            if (request == null) return NotFound();

            return View(request);
        }

        // POST: Start work (Assigned -> InProgress)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartWork(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return NotFound("Maintenance staff profile not found.");

            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null || request.MaintenanceStaffId != staff.Id) return NotFound();

            if (request.Status != "Assigned")
            {
                TempData["Error"] = "Cannot start work: request is not in 'Assigned' status.";
                return RedirectToAction("AssignedRequests");
            }

            request.Status = "InProgress";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Work started. Status updated to InProgress.";
            return RedirectToAction("AssignedRequests");
        }

        // POST: Resolve (InProgress -> Resolved)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return NotFound("Maintenance staff profile not found.");

            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null || request.MaintenanceStaffId != staff.Id) return NotFound();

            if (request.Status != "InProgress")
            {
                TempData["Error"] = "Cannot resolve: request is not in 'InProgress' status.";
                return RedirectToAction("AssignedRequests");
            }

            request.Status = "Resolved";
            request.ResolvedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Request resolved successfully.";
            return RedirectToAction("AssignedRequests");
        }
    }
}
