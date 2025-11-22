using AuthorizationForm.Controllers;
using AuthorizationForm.Data;
using AuthorizationForm.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthorizationForm.Middleware
{
    public class SetupCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SetupCheckMiddleware> _logger;

        public SetupCheckMiddleware(RequestDelegate next, ILogger<SetupCheckMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Skip setup check for setup pages, forms, account pages, and static files
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.StartsWith("/setup") || 
                path.StartsWith("/account/login") ||
                path.StartsWith("/account/register") ||
                path.StartsWith("/requests/create") ||  // Allow access to create form
                path.StartsWith("/requests/search") ||   // Allow search API
                path.StartsWith("/_") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/lib") ||
                path.StartsWith("/images") ||
                path.StartsWith("/favicon"))
            {
                await _next(context);
                return;
            }

            // Only check setup for admin/management pages, not for regular forms
            // Allow users to fill forms even without setup
            if (path.StartsWith("/admin") || 
                path.StartsWith("/manager") ||
                path.StartsWith("/requests") && !path.StartsWith("/requests/create") && !path.StartsWith("/requests/search"))
            {
                try
                {
                    var isSetupNeeded = await SetupController.IsSetupNeededAsync(dbContext, userManager, roleManager);
                    
                    if (isSetupNeeded)
                    {
                        _logger.LogInformation("Setup is needed for management pages, redirecting to setup wizard");
                        context.Response.Redirect("/Setup");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking setup status");
                    // Continue - don't block the request
                }
            }

            await _next(context);
        }
    }

    public static class SetupCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseSetupCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SetupCheckMiddleware>();
        }
    }
}

