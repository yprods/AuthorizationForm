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
            // Ensure database is created and migrations are applied
            try
            {
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not ensure database is created: {ex.Message}");
                // Continue - database might already exist or connection issue
            }
            
            // For MySQL, we'll use migrations instead of raw SQL
            // The EmailTemplates table will be created via migrations
            try
            {
                // Check if EmailTemplates table exists (MySQL way)
                var connection = context.Database.GetDbConnection();
                bool wasOpen = connection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                    connection.Open();
                
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'EmailTemplates'";
                var result = command.ExecuteScalar();
                var tableExists = result != null && Convert.ToInt32(result) > 0;
                
                if (!tableExists)
                {
                    // Create EmailTemplates table (MySQL syntax)
                    var createCommand = connection.CreateCommand();
                    createCommand.CommandText = @"
                        CREATE TABLE EmailTemplates (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            Name VARCHAR(255) NOT NULL,
                            Description TEXT,
                            TriggerType INT NOT NULL,
                            Subject VARCHAR(500) NOT NULL,
                            Body TEXT NOT NULL,
                            IsActive TINYINT(1) NOT NULL DEFAULT 1,
                            CreatedAt DATETIME(6) NOT NULL,
                            UpdatedAt DATETIME(6) NOT NULL,
                            CreatedById VARCHAR(255) NOT NULL,
                            RecipientType VARCHAR(50) NOT NULL DEFAULT 'User',
                            CustomRecipients TEXT,
                            FOREIGN KEY (CreatedById) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci";
                    createCommand.ExecuteNonQuery();
                }
                
                if (!wasOpen)
                    connection.Close();
            }
            catch (Exception ex)
            {
                // If table already exists or other error, continue
                // The EnsureCreated() should handle it
                Console.WriteLine($"Warning: Could not create EmailTemplates table: {ex.Message}");
            }

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
                            var roleResult = userManager.AddToRoleAsync(admin, "Admin").Result;
                            Console.WriteLine($"SUCCESS: Created admin user '{adminUserConfig.Username}' with password '{password}'");
                            if (!roleResult.Succeeded)
                            {
                                Console.WriteLine($"WARNING: Failed to add role Admin to {adminUserConfig.Username}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                            // Log errors if creation failed - but don't throw, just log
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            Console.WriteLine($"ERROR: Failed to create admin user {adminUserConfig.Username}: {errors}");
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
                        var defaultPassword = "Qa123123!@#@WS";
                        var result = userManager.CreateAsync(admin, defaultPassword).Result;
                        if (result.Succeeded)
                        {
                            var roleResult = userManager.AddToRoleAsync(admin, "Admin").Result;
                            Console.WriteLine($"SUCCESS: Created default admin user 'admin' with password '{defaultPassword}'");
                            if (!roleResult.Succeeded)
                            {
                                Console.WriteLine($"WARNING: Failed to add role Admin to admin: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                            // Log errors but don't throw - application can continue
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            Console.WriteLine($"ERROR: Failed to create default admin user: {errors}");
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

