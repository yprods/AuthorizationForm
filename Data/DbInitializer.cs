using AuthorizationForm.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationForm.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
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

            // Create default admin user
            if (userManager.FindByEmailAsync("admin@example.com").Result == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FullName = "מנהל מערכת",
                    IsAdmin = true,
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(admin, "Admin@123").Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(admin, "Admin").Wait();
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

