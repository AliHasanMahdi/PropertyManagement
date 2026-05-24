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

        // PAGE 1: Lists maintenance requests assigned to the currently logged-in maintenance staff
        // URL would be: /MaintenanceStaff/AssignedRequests
        public async Task<IActionResult> AssignedRequests()
        {
            // 1. Get the guy who is currently logged into the website
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // Not logged in? Send them to the login page!

            // 2. Find this person's record in the MaintenanceStaffs table using their email
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
                .Include(m => m.Tenant) // SQL JOIN: Bring in tenant details (name, phone, etc.) so it's not null
                .Include(m => m.Unit)   // SQL JOIN: Bring in the unit details (apartment number, etc.)
                .Where(m => m.MaintenanceStaffId == staff.Id) // Filter: Only get MY jobs
                .OrderByDescending(m => m.CreatedAt) // Sort: Newest requests at the top
                .ToListAsync(); // Run the query and put it in a list

            // 4. Send the list of requests over to the HTML view (AssignedRequests.cshtml)
            return View(requests);
        }

        // PAGE 2: Details of a single assigned request
        // URL looks like: /MaintenanceStaff/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Same check as above: who is logged in?
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Find their staff profile again
            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return RedirectToAction("Dashboard"); // Profile auto-created on Dashboard

            // Find the specific request matching the ID from the URL *AND* make sure it belongs to this staff member
            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .Include(m => m.MaintenanceStaff)
                .FirstOrDefaultAsync(m => m.Id == id && m.MaintenanceStaffId == staff.Id); // Security check: don't let them spy on other staff members' jobs!

            if (request == null) return NotFound(); // Request doesn't exist or doesn't belong to them

            // Send the single request object to Details.cshtml
            return View(request);
        }

        // BUTTON ACTION 1: Start work (Changes status from "Assigned" -> "InProgress")
        // This is a POST request (triggered by clicking a form button, not just visiting a URL)
        [HttpPost]
        [ValidateAntiForgeryToken] // Anti-hacker security token check
        public async Task<IActionResult> StartWork(int id)
        {
            // Check user identity (same routine as before)
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return RedirectToAction("Dashboard"); // Profile auto-created on Dashboard

            // Find the ticket by its ID
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null || request.MaintenanceStaffId != staff.Id) return NotFound();

            // Error check: You can't "Start" a job if it's already in progress or already fixed!
            if (request.Status != "Assigned")
            {
                TempData["Error"] = "Cannot start work: request is not in 'Assigned' status."; // TempData saves a temporary alert message for the next page load
                return RedirectToAction("AssignedRequests"); // Refresh the page/list
            }

            // Change the status string to InProgress
            request.Status = "InProgress";

            // SAVE CHANGES TO DATABASE (If you forget this, nothing actually updates in SQL!)
            await _context.SaveChangesAsync();

            TempData["Success"] = "Work started. Status updated to InProgress.";
            return RedirectToAction("AssignedRequests"); // Go back to the main list
        }

        // BUTTON ACTION 2: Resolve (Changes status from "InProgress" -> "Resolved")
        [HttpPost]
        [ValidateAntiForgeryToken] // Security token again
        public async Task<IActionResult> Resolve(int id)
        {
            // Identity checks... (standard boilerplate stuff)
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var staff = await _context.MaintenanceStaffs
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null) return RedirectToAction("Dashboard"); // Profile auto-created on Dashboard

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
            return RedirectToAction("AssignedRequests"); // Send them back to their dashboard list
        }
        // Alias so the navbar link /MaintenanceStaff/MyRequests works
        public async Task<IActionResult> MyRequests()
        {
            return await AssignedRequests();
        }

    }
}
