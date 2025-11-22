using AuthorizationForm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AuthorizationForm.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IOptions<ActiveDirectorySettings> _adSettings;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IOptions<ActiveDirectorySettings> adSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _adSettings = adSettings;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null, string? username = null)
        {
            // Check if user is already signed in via cookie
            if (_signInManager.IsSignedIn(User))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                return await RedirectToLocal(returnUrl, currentUser);
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
                        var windowsUsername = windowsIdentity.Name?.Split('\\').LastOrDefault() ?? windowsIdentity.Name;
                        var user = await _userManager.FindByNameAsync(windowsUsername);
                        
                        if (user != null)
                        {
                            await _signInManager.SignInAsync(user, true);
                            return await RedirectToLocal(returnUrl, user);
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
            ViewData["IsAdEnabled"] = _adSettings.Value?.Enabled == true;
            ViewData["Username"] = username; // Pre-fill username if provided (from setup wizard)
            return View(new LoginViewModel { Email = username ?? "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["IsAdEnabled"] = _adSettings.Value?.Enabled == true;

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
                            
                            // Refresh sign in to update claims
                            await _signInManager.RefreshSignInAsync(user);
                            
                            // Redirect based on role
                            return await RedirectToLocal(returnUrl, user);
                        }
                        else
                        {
                            _logger.LogWarning($"SignIn failed for {user.UserName}. Result: {result}");
                            ModelState.AddModelError(string.Empty, "נסיון התחברות לא תקין. אנא בדוק את שם המשתמש והסיסמה.");
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
        [AllowAnonymous]
        public async Task<IActionResult> LogoutGet()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            // Only allow registration when AD is disabled
            if (_adSettings.Value?.Enabled == true)
            {
                _logger.LogWarning("Registration attempted but AD is enabled");
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Only allow registration when AD is disabled
            if (_adSettings.Value?.Enabled == true)
            {
                _logger.LogWarning("Registration attempted but AD is enabled");
                ModelState.AddModelError(string.Empty, "הרשמה לא זמינה כאשר Active Directory מופעל");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError(nameof(model.Username), "שם משתמש זה כבר קיים במערכת");
                    return View(model);
                }

                // Check if email already exists
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "כתובת אימייל זו כבר קיימת במערכת");
                    return View(model);
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FullName = model.FullName,
                    EmailConfirmed = true, // Auto-confirm email when AD is disabled
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"New user registered: {user.UserName} ({user.Email})");

                    // Assign "User" role to new registered users
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation($"Assigned 'User' role to {user.UserName}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to assign 'User' role to {user.UserName}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }

                    // Sign in the user automatically after registration
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    // Refresh sign in to update claims immediately (including roles)
                    await _signInManager.RefreshSignInAsync(user);
                    
                    _logger.LogInformation($"User {user.UserName} signed in after registration");

                    // Redirect based on role (new users start as regular users)
                    return await RedirectToLocal(null, user);
                }

                // Add errors from Identity
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        private async Task<IActionResult> RedirectToLocal(string? returnUrl, ApplicationUser? user = null)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // Get user if not provided
            if (user == null)
            {
                user = await _userManager.GetUserAsync(User);
            }
            
            if (user != null)
            {
                // Check if user is Admin
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (isAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                
                // Check if user is Manager
                var isManager = await _userManager.IsInRoleAsync(user, "Manager");
                if (isManager)
                {
                    return RedirectToAction("Index", "Manager");
                }
            }
            
            // Default: redirect to requests page
            return RedirectToAction("Index", "Requests");
        }
    }

}

