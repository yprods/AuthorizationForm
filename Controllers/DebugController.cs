using AuthorizationForm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationForm.Controllers
{
    [AllowAnonymous]
    public class DebugController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DebugController> _logger;

        public DebugController(UserManager<ApplicationUser> userManager, ILogger<DebugController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> CheckUser()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Json(new { 
                    authenticated = false, 
                    message = "לא מחובר" 
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { 
                    authenticated = true, 
                    user = "null",
                    message = "משתמש לא נמצא" 
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Json(new
            {
                authenticated = true,
                username = user.UserName,
                email = user.Email,
                isAdminProperty = user.IsAdmin,
                isManagerProperty = user.IsManager,
                roles = roles,
                isAdminRole = isAdmin,
                isManagerRole = isManager,
                claims = claims,
                userClaims = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> FixAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "משתמש לא נמצא" });
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && user.IsAdmin)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Json(new { success = true, message = "תפקיד Admin נוסף למשתמש" });
            }

            return Json(new { success = false, message = $"משתמש כבר יש לו תפקיד Admin: {isAdmin}, IsAdmin property: {user.IsAdmin}" });
        }
    }
}

