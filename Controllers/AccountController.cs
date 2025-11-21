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
                    _logger.LogInformation($"Attempting login for user: {user.UserName}, User found: {user != null}");
                    
                    // Verify password
                    var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                    _logger.LogInformation($"Password valid for {user.UserName}: {passwordValid}");
                    
                    if (passwordValid)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
                        _logger.LogInformation($"SignIn result for {user.UserName}: {result.Succeeded}");
                        
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User {user.UserName} logged in successfully.");
                        
                        // Verify user has Admin role
                        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                        _logger.LogInformation($"User {user.UserName} - IsAdmin: {user.IsAdmin}, Has Admin Role: {isAdmin}");
                        
                        // If user should be admin but doesn't have role, add it
                        if (user.IsAdmin && !isAdmin)
                        {
                            await _userManager.AddToRoleAsync(user, "Admin");
                            _logger.LogInformation($"Added Admin role to user {user.UserName}");
                            // Sign in again to refresh claims with new role
                            await _signInManager.SignOutAsync();
                            await _signInManager.SignInAsync(user, model.RememberMe);
                        }
                        
                        // Refresh sign-in to ensure roles are loaded in claims
                        // This is important because roles need to be in claims for User.IsInRole to work
                        var roles = await _userManager.GetRolesAsync(user);
                        _logger.LogInformation($"User {user.UserName} roles: {string.Join(", ", roles)}");
                        
                        // Sign out and sign in again to refresh claims
                        await _signInManager.SignOutAsync();
                        await _signInManager.SignInAsync(user, model.RememberMe);
                        
                        return RedirectToLocal(returnUrl);
                    }
                        else
                        {
                            _logger.LogWarning($"SignIn failed for {user.UserName}. Result: {result}");
                            ModelState.AddModelError(string.Empty, $"התחברות נכשלה. שגיאה: {result}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid password for user: {user.UserName}");
                        ModelState.AddModelError(string.Empty, "נסיון התחברות לא תקין. אנא בדוק את שם המשתמש והסיסמה.");
                    }
                }
                else
                {
                    _logger.LogWarning($"User not found: {model.Email}");
                    ModelState.AddModelError(string.Empty, "נסיון התחברות לא תקין. אנא בדוק את שם המשתמש והסיסמה.");
                }
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

