using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.Models;

namespace PropertyManagement.MVC.Controllers
{
    // Only let people in if they are logged in AND have the "MaintenanceStaff" role.
    [Authorize(Roles = "MaintenanceStaff")]
    public class MaintenanceStaffController : Controller
    {
        // Global variables to hold our database connection and user manager stuff
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Constructor: This is where dependency injection injects the DB and User system so we can actually use them below
        public MaintenanceStaffController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        //fixing dashboard 
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);

            // Auto-create profile if missing (handles seed data mismatch or new registrations)
            if (staff == null)
            {
                staff = new PropertyManagement.API.Models.MaintenanceStaff
                {
                    FullName          = user.Email ?? "Staff",
                    Email             = user.Email ?? string.Empty,
                    Phone             = string.Empty,
                    SkillType         = "General",
                    AvailabilityStatus = "Available"
                };
                _context.MaintenanceStaffs.Add(staff);
                await _context.SaveChangesAsync();
            }

            ViewBag.PendingCount = await _context.MaintenanceRequests
                .CountAsync(m => m.MaintenanceStaffId == staff.Id && m.Status == "Assigned");

            ViewBag.InProgressCount = await _context.MaintenanceRequests
                .CountAsync(m => m.MaintenanceStaffId == staff.Id && m.Status == "InProgress");

            ViewBag.ResolvedCount = await _context.MaintenanceRequests
                .CountAsync(m => m.MaintenanceStaffId == staff.Id && m.Status == "Resolved");

            return View();
        }

        
        public async Task<IActionResult> AssignedRequests()
        {
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); 

            
            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null)
            {
                staff = new PropertyManagement.API.Models.MaintenanceStaff
                {
                    FullName          = user.Email ?? "Staff",
                    Email             = user.Email ?? string.Empty,
                    Phone             = string.Empty,
                    SkillType         = "General",
                    AvailabilityStatus = "Available"
                };
                _context.MaintenanceStaffs.Add(staff);
                await _context.SaveChangesAsync();
            }

            // 3. Go to the MaintenanceRequests table and grab all jobs assigned to this specific staff ID
            var requests = await _context.MaintenanceRequests
                .Include(m => m.Tenant) 
                .Include(m => m.Unit)  
                .Where(m => m.MaintenanceStaffId == staff.Id) 
                .OrderByDescending(m => m.CreatedAt) 
                .ToListAsync(); 

            // 4. Send the list of requests over to the HTML view (AssignedRequests.cshtml)
            return View(requests);
        }

        // PAGE 2: Details of a single assigned request
        public async Task<IActionResult> Details(int id)
        {
            // Same check as above: who is logged in?
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Find their staff profile again
            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return RedirectToAction("Dashboard"); 

            // Find the specific request matching the ID from the URL *AND* make sure it belongs to this staff member
            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.MaintenanceStaff)
                .FirstOrDefaultAsync(m => m.Id == id && m.MaintenanceStaffId == staff.Id);

            if (request == null) return NotFound(); 

            // Send the single request object to Details.cshtml
            return View(request);
        }

        // BUTTON ACTION 1: Start work (Changes status from "Assigned" -> "InProgress")
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> StartWork(int id)
        {
            // Check user identity (same routine as before)
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return RedirectToAction("Dashboard"); 

            // Find the ticket by its ID
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null || request.MaintenanceStaffId != staff.Id) return NotFound();

            // Error check: You can't "Start" a job if it's already in progress or already fixed!
            if (request.Status != "Assigned")
            {
                TempData["Error"] = "Cannot start work: request is not in 'Assigned' status."; 
                return RedirectToAction("AssignedRequests"); 
            }

            // Change the status string to InProgress
            request.Status = "InProgress";

            // SAVE CHANGES TO DATABASE (If you forget this, nothing actually updates in SQL!)
            await _context.SaveChangesAsync();

            TempData["Success"] = "Work started. Status updated to InProgress.";
            return RedirectToAction("AssignedRequests"); 
        }

        // BUTTON ACTION 2: Resolve (Changes status from "InProgress" -> "Resolved")
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Resolve(int id)
        {
            // Identity checks... (standard boilerplate stuff)
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return RedirectToAction("Dashboard"); 

            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null || request.MaintenanceStaffId != staff.Id) return NotFound();

            // Error check: You can only resolve a job if you actually started working on it ("InProgress")
            if (request.Status != "InProgress")
            {
                TempData["Error"] = "Cannot resolve: request is not in 'InProgress' status.";
                return RedirectToAction("AssignedRequests");
            }

            // Update status and stamp the current date/time so we know when it was fixed
            request.Status = "Resolved";
            request.ResolvedAt = DateTime.Now;

            // Save it to the database!
            await _context.SaveChangesAsync();

            TempData["Success"] = "Request resolved successfully.";
            return RedirectToAction("AssignedRequests"); 
        }
        // Alias so the navbar link /MaintenanceStaff/MyRequests works
        public async Task<IActionResult> MyRequests()
        {
            return await AssignedRequests();
        }

    }
}
