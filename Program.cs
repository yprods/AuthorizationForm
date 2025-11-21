using AuthorizationForm.Data;
using AuthorizationForm.Models;
using AuthorizationForm.Services;
using AuthorizationForm.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // Add role claims to user claims
    options.ClaimsIdentity.RoleClaimType = System.Security.Claims.ClaimTypes.Role;
    options.ClaimsIdentity.UserNameClaimType = System.Security.Claims.ClaimTypes.Name;
    options.ClaimsIdentity.UserIdClaimType = System.Security.Claims.ClaimTypes.NameIdentifier;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Authentication - Windows Authentication with Cookie fallback
builder.Services.AddAuthentication(options =>
{
    // Use Cookies as default, Negotiate (Windows Auth) only when available
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
    options.DefaultSignInScheme = "Cookies";
    options.DefaultAuthenticateScheme = "Cookies";
})
.AddNegotiate("Negotiate", options =>
{
    // Configure Windows Authentication - optional, only if available
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        options.PersistKerberosCredentials = true;
        options.PersistNtlmCredentials = true;
        // Allow anonymous access if Windows Auth fails
        options.Events = new Microsoft.AspNetCore.Authentication.Negotiate.NegotiateEvents
        {
            OnChallenge = context =>
            {
                // If Windows Auth challenge fails, allow anonymous (middleware will handle manual login)
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Windows Auth challenge failed, allowing anonymous access");
                return Task.CompletedTask;
            },
            OnAuthenticated = context =>
            {
                // Log successful authentication
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation($"Windows Authentication succeeded for: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    }
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Allow anonymous access - only specific pages require authentication
builder.Services.AddAuthorization(options =>
{
    // No fallback policy - allow anonymous access by default
    // Use [Authorize] attribute on specific controllers/actions that need authentication
});

// Add memory cache for AD queries
builder.Services.AddMemoryCache();

// Add custom services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPdfService, PdfService>();
// Register base AD service
builder.Services.AddScoped<ActiveDirectoryService>();
// Register cached wrapper
builder.Services.AddScoped<IActiveDirectoryService>(sp =>
{
    var adService = sp.GetRequiredService<ActiveDirectoryService>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    var logger = sp.GetRequiredService<ILogger<CachedActiveDirectoryService>>();
    return new CachedActiveDirectoryService(adService, cache, logger);
});
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Configure Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<ActiveDirectorySettings>(builder.Configuration.GetSection("ActiveDirectory"));
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));

// Add session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAutoLogin(); // Auto-login via Windows Authentication (must be after UseAuthentication)
app.UseAuthorization();
app.UseSession();

// Set default culture to Hebrew
var supportedCultures = new[] { "he-IL" };
var supportedUICultures = new[] { "he-IL" };

app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture("he-IL");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedUICultures);
});

// Map API controllers first (for attribute routing like [Route])
app.MapControllers();

// Map default MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure Account/Login allows anonymous access
app.MapControllerRoute(
    name: "login",
    pattern: "Account/Login",
    defaults: new { controller = "Account", action = "Login" });

// Seed database
var scope = app.Services.CreateScope();
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting database initialization...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var adminSettings = services.GetRequiredService<IOptions<AdminSettings>>();
        DbInitializer.Initialize(context, userManager, roleManager, adminSettings);
        logger.LogInformation("Database initialization completed successfully.");
        
        // Verify admin user exists
        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser != null)
        {
            var isAdmin = await userManager.IsInRoleAsync(adminUser, "Admin");
            logger.LogInformation($"Admin user 'admin' exists. IsAdmin role: {isAdmin}");
            
            // Test password
            var passwordValid = await userManager.CheckPasswordAsync(adminUser, "Qa123123!@#@WS");
            logger.LogInformation($"Admin user 'admin' password check: {passwordValid}");
        }
        else
        {
            logger.LogWarning("Admin user 'admin' NOT FOUND! Check DbInitializer logs.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred seeding the DB. Application will continue but database may not be properly initialized. Error: {Message}", ex.Message);
        // Don't crash the application - log and continue
        // The database will be created on first access if needed
    }
    finally
    {
        scope.Dispose();
    }
}

app.Run();

