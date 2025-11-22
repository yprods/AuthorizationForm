using AuthorizationForm.Data;
using AuthorizationForm.Models;
using AuthorizationForm.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

namespace AuthorizationForm.Middleware
{
    public class AutoLoginMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AutoLoginMiddleware> _logger;

        public AutoLoginMiddleware(RequestDelegate next, ILogger<AutoLoginMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IActiveDirectoryService adService,
            IOptions<ActiveDirectorySettings> adSettings)
        {
            // Skip static files
            if (context.Request.Path.StartsWithSegments("/css")
                || context.Request.Path.StartsWithSegments("/js")
                || context.Request.Path.StartsWithSegments("/lib")
                || context.Request.Path.StartsWithSegments("/images"))
            {
                await _next(context);
                return;
            }

            // IMPORTANT: Skip auto-login on login/register pages and POST requests to Account
            // This allows manual login and registration to work properly
            if (context.Request.Path.StartsWithSegments("/Account/Login") || 
                context.Request.Path.StartsWithSegments("/Account/Register") ||
                context.Request.Path.StartsWithSegments("/Account/AccessDenied") ||
                (context.Request.Path.StartsWithSegments("/Account") && context.Request.Method == "POST"))
            {
                await _next(context);
                return;
            }

            // Check if already signed in via cookie
            if (context.User != null)
            {
                var isSignedInViaCookie = signInManager.IsSignedIn(context.User);
                if (isSignedInViaCookie)
                {
                    await _next(context);
                    return;
                }
            }

            // Check for Windows Authentication - process even on login page
            System.Security.Principal.WindowsIdentity? windowsIdentity = null;
            string? username = null;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                windowsIdentity = context.User?.Identity as System.Security.Principal.WindowsIdentity;
            }
            
            // Also check if user is authenticated via Negotiate authentication or any Windows authentication
            var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
            var authenticationType = context.User?.Identity?.AuthenticationType;
            var userNameClaim = context.User?.Identity?.Name;
            
            string? windowsIdentityName = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && windowsIdentity != null)
            {
                try
                {
                    windowsIdentityName = windowsIdentity.Name;
                }
                catch (PlatformNotSupportedException)
                {
                    windowsIdentityName = null;
                }
            }
            
            _logger.LogInformation($"User authentication check - IsAuthenticated: {isAuthenticated}, Type: {authenticationType}, Name: {userNameClaim ?? "null"}, WindowsIdentity: {windowsIdentityName ?? "null"}");
            
            // Get username from Windows Identity or from authenticated user
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && windowsIdentity != null)
            {
                try
                {
                    if (windowsIdentity.IsAuthenticated)
                    {
                        username = windowsIdentity.Name?.Split('\\').LastOrDefault() ?? windowsIdentity.Name;
                        _logger.LogInformation($"Windows Authentication detected for user: {username}");
                    }
                }
                catch (PlatformNotSupportedException)
                {
                    // Not on Windows platform, skip Windows Identity
                }
            }
            else if (isAuthenticated && !string.IsNullOrEmpty(userNameClaim))
            {
                // Try to get username from authenticated identity (may be from Negotiate)
                username = userNameClaim.Split('\\').LastOrDefault() ?? userNameClaim;
                _logger.LogInformation($"Using authenticated username from Identity: {username}");
            }
            
            if (!string.IsNullOrEmpty(username))
            {
                try
                {
                    // Get or create user from AD
                    var user = await userManager.FindByNameAsync(username);
                    
                    if (user == null)
                    {
                        // Try to import user from AD automatically (only if AD is enabled)
                        if (adSettings.Value?.Enabled == true && 
                            !string.IsNullOrWhiteSpace(adSettings.Value?.LdapPath) &&
                            !adSettings.Value.LdapPath.Contains("yourdomain.com"))
                        {
                            try
                            {
                                var adUserInfo = await adService.GetUserInfoAsync(username);
                                if (adUserInfo != null)
                                {
                                    var managementGroup = adSettings.Value?.ManagementGroup ?? "";
                                    var isManager = !string.IsNullOrEmpty(managementGroup) && await adService.IsUserInGroupAsync(username, managementGroup);
                                    
                                    user = new ApplicationUser
                                    {
                                        UserName = adUserInfo.Username,
                                        Email = adUserInfo.Email ?? $"{adUserInfo.Username}@example.com",
                                        FullName = adUserInfo.FullName,
                                        Department = adUserInfo.Department,
                                        IsManager = isManager,
                                        IsAdmin = false,
                                        EmailConfirmed = true
                                    };

                                    // Generate a random password
                                    var password = Guid.NewGuid().ToString() + "!Aa1";
                                    var result = await userManager.CreateAsync(user, password);
                                    
                                    if (result.Succeeded)
                                    {
                                        if (isManager)
                                        {
                                            await userManager.AddToRoleAsync(user, "Manager");
                                        }
                                        else
                                        {
                                            await userManager.AddToRoleAsync(user, "User");
                                        }
                                        
                                        _logger.LogInformation($"Auto-imported user {username} from AD");
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Failed to auto-import user {username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                                        user = null; // Don't sign in if creation failed
                                    }
                                }
                                else
                                {
                                    _logger.LogInformation($"User {username} not found in AD - will require manual login");
                                }
                            }
                            catch (Exception adEx)
                            {
                                _logger.LogWarning(adEx, $"Could not import user {username} from AD: {adEx.Message}");
                                // Continue - user can still login manually
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"AD is disabled or not configured - user {username} will require manual login");
                        }
                    }
                    
                    if (user != null)
                    {
                        // Sign in the user automatically
                        await signInManager.SignInAsync(user, true);
                        _logger.LogInformation($"Auto-logged in user {username} via Windows Authentication");
                        
                        // Redirect to home page
                        if (context.Request.Path != "/" && !context.Request.Path.StartsWithSegments("/Home") && !context.Request.Path.StartsWithSegments("/Account/Login"))
                        {
                            context.Response.Redirect("/");
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"User {username} not found in system and could not be imported - redirecting to manual login");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in auto-login for {username}: {ex.Message}");
                    // If error, allow access to login page for manual login
                }
            }
            else
            {
                // No Windows Authentication detected - allow anonymous access
                _logger.LogDebug($"No Windows Authentication detected. User: {context.User?.Identity?.Name ?? "Anonymous"}, Type: {authenticationType}, IsAuthenticated: {isAuthenticated}, Path: {context.Request.Path}");
            }

            // Continue to next middleware
            await _next(context);
        }
    }

    public static class AutoLoginMiddlewareExtensions
    {
        public static IApplicationBuilder UseAutoLogin(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AutoLoginMiddleware>();
        }
    }
}
