using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.API.Data;

namespace PropertyManagement.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                var roles = await _userManager.GetRolesAsync(user!);

                if (roles.Contains("PropertyManager"))
                    return RedirectToAction("Dashboard", "PropertyManager");
                else if (roles.Contains("MaintenanceStaff"))
                    return RedirectToAction("AssignedRequests", "MaintenanceStaff");
                else
                    return RedirectToAction("Dashboard", "Tenant");
            }
            ModelState.AddModelError("", "Invalid login attempt");
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string role)
        {
            // Create roles if they don't exist
            string[] roles = { "PropertyManager", "MaintenanceStaff", "Tenant" };
            foreach (var r in roles)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole(r));
            }

            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}