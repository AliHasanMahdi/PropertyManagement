using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.Models;
using PropertyManagement.MVC.Services;
using PropertyManagement.MVC.ViewModels.Account;

namespace PropertyManagement.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApiClientService _apiClient;
        private readonly TokenService _tokenService;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApiClientService apiClient,
            TokenService tokenService,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _apiClient = apiClient;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Step 1: Sign in with Identity (for MVC cookie auth)
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Step 2: Get JWT token from API (for API calls)
            var (success, token, roles, error) =
                await _apiClient.LoginAsync(model.Email, model.Password);

            if (success && token != null && roles != null)
            {
                // Save JWT token in cookie for API calls
                _tokenService.SaveToken(token, model.Email, roles);
            }

            // Step 3: Redirect based on role
            var user = await _userManager.FindByEmailAsync(model.Email);
            var userRoles = await _userManager.GetRolesAsync(user!);

            if (userRoles.Contains("PropertyManager"))
                return RedirectToAction("Dashboard", "PropertyManager");
            else if (userRoles.Contains("MaintenanceStaff"))
                return RedirectToAction("Dashboard", "MaintenanceStaff");
            else
                return RedirectToAction("Dashboard", "Tenant");
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Create roles if they don't exist
            string[] roles = { "PropertyManager", "MaintenanceStaff", "Tenant" };
            foreach (var r in roles)
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole(r));

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign the selected role to the new user
                await _userManager.AddToRoleAsync(user, model.Role);

                
                if (model.Role == "MaintenanceStaff")
                {
                    // Prevent duplicate MaintenanceStaff records by checking email
                    var exists = await _context.MaintenanceStaffs
                        .AnyAsync(s => s.Email == model.Email);
                    if (!exists)
                    {
                        var staff = new MaintenanceStaff
                        {
                            Email = model.Email,
                            FullName = model.Email,
                            SkillType = "General",
                            AvailabilityStatus = "Available",
                            Phone = string.Empty
                        };

                        // Create and save the MaintenanceStaff profile
                        _context.MaintenanceStaffs.Add(staff);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Success"] = "Account created! Please login.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            // Clear JWT cookie
            _tokenService.ClearToken();

            // Clear Identity cookie
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}