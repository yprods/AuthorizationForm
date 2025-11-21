using AuthorizationForm.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;

namespace AuthorizationForm.Data
{
    public static class DbInitializer
    {
        public static void Initialize(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            IOptions<AdminSettings> adminSettings)
        {
            context.Database.EnsureCreated();

            // Create roles
            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
            }

            if (!roleManager.RoleExistsAsync("Manager").Result)
            {
                roleManager.CreateAsync(new IdentityRole("Manager")).Wait();
            }

            if (!roleManager.RoleExistsAsync("User").Result)
            {
                roleManager.CreateAsync(new IdentityRole("User")).Wait();
            }

            // Create admin users from configuration
            var adminConfig = adminSettings.Value;
            
            if (adminConfig.AdminUsers != null && adminConfig.AdminUsers.Any())
            {
                foreach (var adminUserConfig in adminConfig.AdminUsers)
                {
                    // Check if admin user exists by username or email
                    var existingAdmin = userManager.FindByNameAsync(adminUserConfig.Username).Result ?? 
                                       userManager.FindByEmailAsync(adminUserConfig.Email).Result;
                    
                    if (existingAdmin == null)
                    {
                        // Create new admin user
                        var admin = new ApplicationUser
                        {
                            UserName = adminUserConfig.Username,
                            Email = adminUserConfig.Email,
                            FullName = adminUserConfig.FullName ?? "מנהל מערכת",
                            IsAdmin = true,
                            IsManager = false,
                            EmailConfirmed = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Create admin user with password from configuration
                        var password = !string.IsNullOrWhiteSpace(adminUserConfig.Password) 
                            ? adminUserConfig.Password 
                            : Guid.NewGuid().ToString() + "!Aa1"; // Fallback password
                            
                        var result = userManager.CreateAsync(admin, password).Result;
                        if (result.Succeeded)
                        {
                            userManager.AddToRoleAsync(admin, "Admin").Wait();
                        }
                        else
                        {
                            // Log errors if creation failed - but don't throw, just log
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            // Use Console.WriteLine as fallback if logger not available
                            Console.WriteLine($"WARNING: Failed to create admin user {adminUserConfig.Username}: {errors}");
                            // Don't throw exception - continue with other admin users or fallback
                        }
                    }
                    else
                    {
                        // Update existing admin to ensure it has admin role and correct settings
                        if (!userManager.IsInRoleAsync(existingAdmin, "Admin").Result)
                        {
                            userManager.AddToRoleAsync(existingAdmin, "Admin").Wait();
                        }
                        existingAdmin.IsAdmin = true;
                        existingAdmin.FullName = adminUserConfig.FullName ?? existingAdmin.FullName;
                        userManager.UpdateAsync(existingAdmin).Wait();
                    }
                }
            }
            else
            {
                // Fallback: Create default admin user if no admin users are configured
                // Check if any admin user exists
                var adminRole = roleManager.FindByNameAsync("Admin").Result;
                bool hasAdminUsers = false;
                if (adminRole != null)
                {
                    var usersInAdminRole = userManager.GetUsersInRoleAsync("Admin").Result;
                    hasAdminUsers = usersInAdminRole.Any();
                }

                // Only create default admin if no admin users exist at all
                if (!hasAdminUsers)
                {
                    var existingAdmin = userManager.FindByNameAsync("admin").Result ?? 
                                       userManager.FindByEmailAsync("admin@example.com").Result;
                    
                    if (existingAdmin == null)
                    {
                        var admin = new ApplicationUser
                        {
                            UserName = "admin",
                            Email = "admin@example.com",
                            FullName = "מנהל מערכת",
                            IsAdmin = true,
                            IsManager = false,
                            EmailConfirmed = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Create admin user with default password: Qa123123!@#@WS
                        var result = userManager.CreateAsync(admin, "Qa123123!@#@WS").Result;
                        if (result.Succeeded)
                        {
                            userManager.AddToRoleAsync(admin, "Admin").Wait();
                        }
                        else
                        {
                            // Log errors but don't throw - application can continue
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            Console.WriteLine($"WARNING: Failed to create default admin user: {errors}");
                            // Application will continue - user can be created manually
                        }
                    }
                }
            }

            // Create default manager
            if (userManager.FindByEmailAsync("manager@example.com").Result == null)
            {
                var manager = new ApplicationUser
                {
                    UserName = "manager@example.com",
                    Email = "manager@example.com",
                    FullName = "מנהל מחלקה",
                    IsManager = true,
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(manager, "Manager@123").Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(manager, "Manager").Wait();
                }
            }

            // Seed employees if needed
            if (!context.Employees.Any())
            {
                context.Employees.AddRange(
                    new Employee { EmployeeId = "EMP001", FirstName = "יוסי", LastName = "כהן", Department = "IT", Email = "yossi@example.com" },
                    new Employee { EmployeeId = "EMP002", FirstName = "שרה", LastName = "לוי", Department = "HR", Email = "sara@example.com" },
                    new Employee { EmployeeId = "EMP003", FirstName = "דוד", LastName = "ישראלי", Department = "כספים", Email = "david@example.com" }
                );
                context.SaveChanges();
            }
        }
    }
}

