using Microsoft.AspNetCore.Mvc;
using PropertyManagement.MVC.Models;
using System.Diagnostics;

namespace PropertyManagement.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Redirect logged-in users straight to their dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("PropertyManager"))
                    return RedirectToAction("Dashboard", "PropertyManager");
                if (User.IsInRole("MaintenanceStaff"))
                    return RedirectToAction("Dashboard", "MaintenanceStaff");
                if (User.IsInRole("Tenant"))
                    return RedirectToAction("Dashboard", "Tenant");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
