using AuthorizationForm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // Check if user is already signed in via cookie
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToLocal(returnUrl);
            }

            // Check if user is authenticated via Windows Authentication
            System.Security.Principal.WindowsIdentity? windowsIdentity = null;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                try
                {
                    windowsIdentity = User.Identity as System.Security.Principal.WindowsIdentity;
                }
                catch (PlatformNotSupportedException)
                {
                    // Not on Windows platform
                }
            }
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) && 
                windowsIdentity != null)
            {
                try
                {
                    if (windowsIdentity.IsAuthenticated)
                    {
                        // User authenticated via Windows - middleware should handle auto-login
                        // Wait briefly and redirect to let middleware process
                        await Task.Delay(100);
                        
                        // Try to get the user and sign in immediately
                        var username = windowsIdentity.Name?.Split('\\').LastOrDefault() ?? windowsIdentity.Name;
                        var user = await _userManager.FindByNameAsync(username);
                        
                        if (user != null)
                        {
                            await _signInManager.SignInAsync(user, true);
                            return RedirectToLocal(returnUrl);
                        }
                        
                        // If user not found, redirect to home to let middleware import them
                        return Redirect("/");
                    }
                }
                catch (PlatformNotSupportedException)
                {
                    // Not on Windows platform, continue to manual login
                }
            }

            // No Windows Authentication - show manual login form
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ShowManualLogin"] = true;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Require all fields to be filled for anonymous users
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "שדה שם משתמש הוא חובה");
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "שדה סיסמה הוא חובה");
            }

            if (ModelState.IsValid)
            {
                // Try to find user by username or email
                var user = await _userManager.FindByNameAsync(model.Email) ?? 
                          await _userManager.FindByEmailAsync(model.Email);
                
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User {user.UserName} logged in.");
                        return RedirectToLocal(returnUrl);
                    }
                }

                ModelState.AddModelError(string.Empty, "נסיון התחברות לא תקין. אנא בדוק את שם המשתמש והסיסמה.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogoutGet()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

}

