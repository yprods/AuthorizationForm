using AuthorizationForm.Data;
using AuthorizationForm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthorizationForm.Controllers
{
    [AllowAnonymous]
    public class SetupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SetupController> _logger;

        public SetupController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<SetupController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // Check if setup is needed
        public static async Task<bool> IsSetupNeededAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // Check if database is accessible
                if (!await context.Database.CanConnectAsync())
                {
                    return true; // Setup needed if database not accessible
                }

                // Check if roles exist
                var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
                if (!adminRoleExists)
                {
                    return true; // Setup needed if roles don't exist
                }

                // Check if any admin user exists
                var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
                if (adminUsers == null || !adminUsers.Any())
                {
                    return true; // Setup needed if no admin exists
                }

                return false; // Setup not needed
            }
            catch
            {
                return true; // If error, assume setup is needed
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Check if setup is already complete
            var isSetupNeeded = await IsSetupNeededAsync(_context, _userManager, _roleManager);
            if (!isSetupNeeded)
            {
                // Setup already complete, redirect to home
                return RedirectToAction("Index", "Home");
            }

            return View(new SetupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();

                // Create roles if they don't exist
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    _logger.LogInformation("Created Admin role");
                }

                if (!await _roleManager.RoleExistsAsync("Manager"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Manager"));
                    _logger.LogInformation("Created Manager role");
                }

                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                    _logger.LogInformation("Created User role");
                }

                // Check if admin user already exists
                var existingAdmin = await _userManager.FindByNameAsync(model.Username);
                if (existingAdmin != null)
                {
                    ModelState.AddModelError(nameof(model.Username), "שם משתמש זה כבר קיים במערכת");
                    return View(model);
                }

                var existingAdminByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingAdminByEmail != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "כתובת אימייל זו כבר קיימת במערכת");
                    return View(model);
                }

                // Create the admin user
                var adminUser = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FullName = model.FullName ?? model.Username,
                    EmailConfirmed = true,
                    IsAdmin = true,
                    IsManager = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, model.Password);

                if (result.Succeeded)
                {
                    // Assign Admin and Manager roles
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    await _userManager.AddToRoleAsync(adminUser, "Manager");

                    _logger.LogInformation($"Setup completed successfully. Admin user '{adminUser.UserName}' created.");

                    TempData["SetupSuccess"] = true;
                    TempData["SetupMessage"] = "ההתקנה הושלמה בהצלחה! משתמש המנהל נוצר.";

                    // Redirect to login - user will need to login with the credentials they just created
                    return RedirectToAction("Login", "Account", new { username = adminUser.UserName });
                }

                // Add errors from Identity
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during setup");
                ModelState.AddModelError(string.Empty, $"שגיאה במהלך ההתקנה: {ex.Message}");
            }

            return View(model);
        }
    }
}

