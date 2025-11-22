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
using System.Linq;
using DotNetEnv;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

// Load .env file if it exists
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure Database - MySQL with SQLite fallback
var useMySql = builder.Configuration.GetValue<bool>("DatabaseSettings:UseMySql", true);
var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (useMySql && !connectionString.Contains("Data Source="))
{
    // Use MySQL
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 33));
    
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, serverVersion, mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));
}
else
{
    // Fallback to SQLite if MySQL connection string contains "Data Source=" or UseMySql is false
    var sqliteConnection = connectionString.Contains("Data Source=") 
        ? connectionString 
        : "Data Source=authorization.db";
    
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(sqliteConnection));
}

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // Configure claim types
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
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
    // Use default redirect behavior - ASP.NET Core will handle redirects automatically
});

// Configure authorization
builder.Services.AddAuthorization(options =>
{
    // Only require authentication for pages marked with [Authorize]
    // Pages marked with [AllowAnonymous] will be accessible without authentication
    options.FallbackPolicy = null; // Allow anonymous by default
});

// Add memory cache for AD queries
builder.Services.AddMemoryCache();

// Add custom services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// Add background reminder service
builder.Services.AddHostedService<ReminderService>();
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

// Enable authentication and authorization
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

// Map default MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map API controllers (for attribute routing like [Route])
app.MapControllers();

// Seed database
var scope = app.Services.CreateScope();
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting database initialization...");
        
        // Test database connection first
        var context = services.GetRequiredService<ApplicationDbContext>();
        try
        {
            // Try to connect to database with timeout
            var canConnect = await Task.Run(async () =>
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    return await context.Database.CanConnectAsync(cts.Token);
                }
                catch
                {
                    return false;
                }
            });
            
            if (!canConnect)
            {
                logger.LogWarning("Cannot connect to database. Attempting to create database...");
                try
                {
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("Database created successfully.");
                }
                catch (Exception createEx)
                {
                    logger.LogError(createEx, "Failed to create database");
                    // Don't throw - allow application to start
                }
            }
            else
            {
                logger.LogInformation("Database connection successful.");
            }
        }
        catch (Exception dbEx)
        {
            logger.LogError(dbEx, "Failed to connect to database. Please check:");
            logger.LogError("1. Database server is running");
            logger.LogError("2. Connection string is correct in appsettings.json or .env file");
            
            if (useMySql && !connectionString.Contains("Data Source="))
            {
                logger.LogError("3. For MySQL: Database exists: CREATE DATABASE authorization_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
                logger.LogError("4. For MySQL: User has proper permissions");
            }
            
            var safeConnectionString = connectionString.Contains("Password=") 
                ? connectionString.Substring(0, connectionString.IndexOf("Password=")) + "Password=***"
                : connectionString;
            logger.LogError($"Connection string: {safeConnectionString}");
            
            // Don't throw - allow application to start but log the error
            logger.LogWarning("Application will continue but database operations may fail. Please fix database connection.");
        }
        
        try
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var adminSettings = services.GetRequiredService<IOptions<AdminSettings>>();
            
            // Only initialize if database is accessible
            try
            {
                if (await context.Database.CanConnectAsync())
                {
                    DbInitializer.Initialize(context, userManager, roleManager, adminSettings);
                    logger.LogInformation("Database initialization completed successfully.");
                    
                    // Verify admin user exists and ensure password matches appsettings.json
                    var adminUser = await userManager.FindByNameAsync("admin");
                    if (adminUser != null)
                    {
                        var isAdmin = await userManager.IsInRoleAsync(adminUser, "Admin");
                        logger.LogInformation($"Admin user 'admin' exists. IsAdmin role: {isAdmin}");
                        
                        // Get password from appsettings.json
                        var adminPassword = adminSettings.Value?.AdminUsers?.FirstOrDefault()?.Password ?? "Qa123456";
                        var passwordValid = await userManager.CheckPasswordAsync(adminUser, adminPassword);
                        logger.LogInformation($"Admin user 'admin' password check with '{adminPassword}': {passwordValid}");
                        
                        // If password doesn't match, reset it
                        if (!passwordValid)
                        {
                            logger.LogWarning($"Admin password doesn't match. Resetting to '{adminPassword}'...");
                            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                            var resetResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
                            if (resetResult.Succeeded)
                            {
                                logger.LogInformation($"✓ Admin user 'admin' password reset successfully to '{adminPassword}'");
                            }
                            else
                            {
                                logger.LogError($"✗ Failed to reset admin password: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                            logger.LogInformation($"✓ Admin password is correct: '{adminPassword}'");
                        }
                    }
                    else
                    {
                        logger.LogWarning("Admin user 'admin' NOT FOUND! Check DbInitializer logs.");
                    }
                }
                else
                {
                    logger.LogWarning("Database not accessible, skipping initialization");
                }
            }
            catch (Exception initEx)
            {
                logger.LogError(initEx, "Error during database initialization");
                // Continue - don't crash the application
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred seeding the DB. Application will continue but database may not be properly initialized. Error: {Message}", ex.Message);
            // Don't crash the application - log and continue
            // The database will be created on first access if needed
        }
    }
    finally
    {
        scope.Dispose();
    }
}

app.Run();

