using AuthorizationForm.Controllers;
using AuthorizationForm.Data;
using AuthorizationForm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationForm.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            ILogger<HomeController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Check if setup is needed only for authenticated users accessing admin/management areas
            // Allow anonymous users to access forms
            if (User.Identity?.IsAuthenticated == true)
            {
                var isSetupNeeded = await SetupController.IsSetupNeededAsync(_context, _userManager, _roleManager);
                if (isSetupNeeded)
                {
                    return RedirectToAction("Index", "Setup");
                }
            }

            // If user is not authenticated, redirect to create form (allow anonymous access)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Create", "Requests");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Redirect Admin users to Admin panel
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            // Redirect Manager users to Manager dashboard
            if (await _userManager.IsInRoleAsync(user, "Manager"))
            {
                return RedirectToAction("Index", "Manager");
            }

            // Regular users go to requests page
            return RedirectToAction("Index", "Requests");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

