using AuthorizationForm.Data;
using AuthorizationForm.Models;
using AuthorizationForm.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;

namespace AuthorizationForm.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IActiveDirectoryService _adService;
        private readonly ActiveDirectorySettings _adSettings;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IActiveDirectoryService adService,
            IOptions<ActiveDirectorySettings> adSettings,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _adService = adService;
            _adSettings = adSettings.Value;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Admin Index: User is null");
                return RedirectToAction("Login", "Account");
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (!isAdmin)
            {
                _logger.LogWarning($"Admin Index: User {currentUser.UserName} is not Admin");
                return Forbid();
            }

            _logger.LogInformation($"Admin Index: User {currentUser.UserName} accessed admin panel");
            return View();
        }

        // Employees Management
        public async Task<IActionResult> Employees()
        {
            return View(await _context.Employees.OrderBy(e => e.LastName).ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateEmployee()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.CreatedAt = DateTime.UtcNow;
                employee.UpdatedAt = DateTime.UtcNow;
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Employees));
            }
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> EditEmployee(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(int id, Employee employee)
        {
            if (id != employee.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    employee.UpdatedAt = DateTime.UtcNow;
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Employees.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Employees));
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                employee.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Employees));
        }

        // Systems Management
        public async Task<IActionResult> Systems()
        {
            return View(await _context.Systems.OrderBy(s => s.Name).ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateSystem()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSystem(ApplicationSystem system)
        {
            if (ModelState.IsValid)
            {
                system.CreatedAt = DateTime.UtcNow;
                system.UpdatedAt = DateTime.UtcNow;
                _context.Add(system);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Systems));
            }
            return View(system);
        }

        [HttpGet]
        public async Task<IActionResult> EditSystem(int? id)
        {
            if (id == null) return NotFound();
            var system = await _context.Systems.FindAsync(id);
            if (system == null) return NotFound();
            return View(system);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSystem(int id, ApplicationSystem system)
        {
            if (id != system.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    system.UpdatedAt = DateTime.UtcNow;
                    _context.Update(system);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Systems.Any(s => s.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Systems));
            }
            return View(system);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSystem(int id)
        {
            var system = await _context.Systems.FindAsync(id);
            if (system != null)
            {
                system.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Systems));
        }

        // Managers Management
        public async Task<IActionResult> Managers()
        {
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var allManagers = managers.Union(admins).Where(u => u.IsManager || u.IsAdmin).ToList();
            return View(allManagers);
        }

        // Users Management
        public async Task<IActionResult> Users()
        {
            var allUsers = _userManager.Users.ToList();
            var usersWithRoles = new List<(ApplicationUser User, IList<string> Roles)>();
            
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add((user, roles));
            }
            
            ViewBag.UsersWithRoles = usersWithRoles;
            return View(allUsers);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string userName, string email, string password, string fullName, string department, bool isManager, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                ModelState.AddModelError("", "שם משתמש הוא חובה");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "אימייל הוא חובה");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "סיסמה היא חובה");
            }
            else if (password.Length < 6)
            {
                ModelState.AddModelError("", "סיסמה חייבת להכיל לפחות 6 תווים");
            }

            if (ModelState.IsValid)
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(userName) ?? 
                                 await _userManager.FindByEmailAsync(email);
                
                if (existingUser != null)
                {
                    ModelState.AddModelError("", $"משתמש עם שם משתמש '{userName}' או אימייל '{email}' כבר קיים במערכת");
                    return View();
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    FullName = fullName,
                    Department = department,
                    IsManager = isManager,
                    IsAdmin = isAdmin,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Add to appropriate role
                    if (isAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                        if (!isManager)
                        {
                            isManager = true; // Admin is also Manager
                            user.IsManager = true;
                            await _userManager.UpdateAsync(user);
                        }
                    }
                    else if (isManager)
                    {
                        await _userManager.AddToRoleAsync(user, "Manager");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    TempData["Success"] = $"משתמש '{userName}' נוצר בהצלחה";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = roles;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string userName, string email, string fullName, string department, bool isManager, bool isAdmin, string? newPassword)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                ModelState.AddModelError("", "שם משתמש הוא חובה");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "אימייל הוא חובה");
            }

            if (ModelState.IsValid)
            {
                // Check if username or email is already taken by another user
                var existingUserByUsername = await _userManager.FindByNameAsync(userName);
                if (existingUserByUsername != null && existingUserByUsername.Id != id)
                {
                    ModelState.AddModelError("", $"שם משתמש '{userName}' כבר תפוס על ידי משתמש אחר");
                    var roles = await _userManager.GetRolesAsync(user);
                    ViewBag.UserRoles = roles;
                    return View(user);
                }

                var existingUserByEmail = await _userManager.FindByEmailAsync(email);
                if (existingUserByEmail != null && existingUserByEmail.Id != id)
                {
                    ModelState.AddModelError("", $"אימייל '{email}' כבר תפוס על ידי משתמש אחר");
                    var roles = await _userManager.GetRolesAsync(user);
                    ViewBag.UserRoles = roles;
                    return View(user);
                }

                // Update user properties
                user.UserName = userName;
                user.Email = email;
                user.FullName = fullName;
                user.Department = department;
                
                // Handle admin/manager roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                
                if (isAdmin && !currentRoles.Contains("Admin"))
                {
                    user.IsAdmin = true;
                    user.IsManager = true; // Admin is also Manager
                    await _userManager.AddToRoleAsync(user, "Admin");
                    if (!currentRoles.Contains("Manager"))
                    {
                        await _userManager.AddToRoleAsync(user, "Manager");
                    }
                    await _userManager.RemoveFromRoleAsync(user, "User");
                }
                else if (!isAdmin && currentRoles.Contains("Admin"))
                {
                    user.IsAdmin = false;
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    if (!isManager)
                    {
                        await _userManager.RemoveFromRoleAsync(user, "Manager");
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }
                
                if (isManager && !currentRoles.Contains("Manager"))
                {
                    user.IsManager = true;
                    await _userManager.AddToRoleAsync(user, "Manager");
                    if (!currentRoles.Contains("Admin"))
                    {
                        await _userManager.RemoveFromRoleAsync(user, "User");
                    }
                }
                else if (!isManager && currentRoles.Contains("Manager") && !currentRoles.Contains("Admin"))
                {
                    user.IsManager = false;
                    await _userManager.RemoveFromRoleAsync(user, "Manager");
                    await _userManager.AddToRoleAsync(user, "User");
                }

                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    // Update password if provided
                    if (!string.IsNullOrWhiteSpace(newPassword) && newPassword.Length >= 6)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                        if (!passwordResult.Succeeded)
                        {
                            foreach (var error in passwordResult.Errors)
                            {
                                ModelState.AddModelError("", $"שגיאה בעדכון סיסמה: {error.Description}");
                            }
                            var roles = await _userManager.GetRolesAsync(user);
                            ViewBag.UserRoles = roles;
                            return View(user);
                        }
                    }

                    TempData["Success"] = $"משתמש '{userName}' עודכן בהצלחה";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = userRoles;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["Error"] = "לא ניתן למחוק את המשתמש הנוכחי";
                return RedirectToAction(nameof(Users));
            }

            // Prevent deleting the last admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "לא ניתן למחוק את המשתמש האחרון עם הרשאת Admin";
                    return RedirectToAction(nameof(Users));
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"משתמש '{user.UserName}' נמחק בהצלחה";
            }
            else
            {
                TempData["Error"] = $"שגיאה במחיקת משתמש: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Users));
        }

        // Form Templates Management
        public async Task<IActionResult> FormTemplates()
        {
            return View(await _context.FormTemplates
                .Include(t => t.CreatedBy)
                .OrderBy(t => t.Name)
                .ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateFormTemplate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFormTemplate(FormTemplate template)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();

                template.CreatedById = currentUser.Id;
                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;
                _context.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(FormTemplates));
            }
            return View(template);
        }

        [HttpGet]
        public async Task<IActionResult> EditFormTemplate(int? id)
        {
            if (id == null) return NotFound();
            var template = await _context.FormTemplates.FindAsync(id);
            if (template == null) return NotFound();
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFormTemplate(int id, FormTemplate template)
        {
            if (id != template.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    template.UpdatedAt = DateTime.UtcNow;
                    _context.Update(template);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.FormTemplates.Any(t => t.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(FormTemplates));
            }
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFormTemplate(int id)
        {
            var template = await _context.FormTemplates.FindAsync(id);
            if (template != null)
            {
                template.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(FormTemplates));
        }

        // Email Templates Management
        public async Task<IActionResult> EmailTemplates()
        {
            return View(await _context.EmailTemplates
                .Include(t => t.CreatedBy)
                .OrderBy(t => t.TriggerType)
                .ThenBy(t => t.Name)
                .ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateEmailTemplate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmailTemplate(EmailTemplate template)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            if (ModelState.IsValid)
            {
                template.CreatedById = currentUser.Id;
                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;
                _context.Add(template);
                await _context.SaveChangesAsync();
                TempData["Success"] = "תבנית אימייל נוצרה בהצלחה";
                return RedirectToAction(nameof(EmailTemplates));
            }
            return View(template);
        }

        [HttpGet]
        public async Task<IActionResult> EditEmailTemplate(int? id)
        {
            if (id == null) return NotFound();
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null) return NotFound();
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmailTemplate(int id, EmailTemplate template)
        {
            if (id != template.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    template.UpdatedAt = DateTime.UtcNow;
                    _context.Update(template);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "תבנית אימייל עודכנה בהצלחה";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.EmailTemplates.Any(t => t.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(EmailTemplates));
            }
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmailTemplate(int id)
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template != null)
            {
                template.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "תבנית אימייל נמחקה בהצלחה";
            }
            return RedirectToAction(nameof(EmailTemplates));
        }

        // Import Users from Active Directory
        [HttpGet]
        public IActionResult ImportUsers()
        {
            return View();
        }

        // Search AD Users - API endpoint for auto-complete
        [HttpGet]
        public async Task<IActionResult> SearchAdUsers(string term, int maxResults = 20)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            // Check if AD is enabled
            if (!_adSettings.Enabled || string.IsNullOrWhiteSpace(_adSettings.LdapPath) || _adSettings.LdapPath.Contains("yourdomain.com"))
            {
                _logger.LogInformation("AD is disabled or not configured - returning empty results");
                return Json(new List<object>());
            }

            try
            {
                var users = await _adService.SearchUsersAsync(term, maxResults);
                var results = users.Select(u => new
                {
                    username = u.Username,
                    fullName = u.FullName,
                    email = u.Email ?? "",
                    department = u.Department ?? "",
                    title = u.Title ?? ""
                }).ToList();

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching AD users: {ex.Message}");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportUserByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "אנא הכנס שם משתמש";
                return RedirectToAction(nameof(ImportUsers));
            }

            // Check if AD is enabled
            if (!_adSettings.Enabled || string.IsNullOrWhiteSpace(_adSettings.LdapPath) || _adSettings.LdapPath.Contains("yourdomain.com"))
            {
                TempData["Error"] = "אקטיב דירקטורי אינו מופעל או לא מוגדר. אנא השתמש ביצירת משתמש ידנית.";
                return RedirectToAction(nameof(ImportUsers));
            }

            try
            {
                var adUserInfo = await _adService.GetUserInfoAsync(username);
                if (adUserInfo == null)
                {
                    TempData["Error"] = $"משתמש {username} לא נמצא באקטיב דירקטורי";
                    return RedirectToAction(nameof(ImportUsers));
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(adUserInfo.Username);
                if (existingUser != null)
                {
                    TempData["Error"] = $"משתמש {username} כבר קיים במערכת";
                    return RedirectToAction(nameof(ImportUsers));
                }

                // Check if user is in management group
                var isInManagementGroup = await _adService.IsUserInGroupAsync(username, _adSettings.ManagementGroup);

                // Create user
                var user = new ApplicationUser
                {
                    UserName = adUserInfo.Username,
                    Email = adUserInfo.Email ?? $"{adUserInfo.Username}@example.com",
                    FullName = adUserInfo.FullName,
                    Department = adUserInfo.Department,
                    IsManager = isInManagementGroup,
                    IsAdmin = false,
                    EmailConfirmed = true
                };

                // Generate a random password (user will need to reset it)
                var password = Guid.NewGuid().ToString() + "!Aa1";
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Add to appropriate role
                    if (isInManagementGroup)
                    {
                        await _userManager.AddToRoleAsync(user, "Manager");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    // Create employee record if needed
                    if (!string.IsNullOrEmpty(adUserInfo.EmployeeId))
                    {
                        var employee = new Employee
                        {
                            EmployeeId = adUserInfo.EmployeeId,
                            FirstName = adUserInfo.FullName.Split(' ')[0],
                            LastName = adUserInfo.FullName.Contains(' ') 
                                ? string.Join(" ", adUserInfo.FullName.Split(' ').Skip(1)) 
                                : "",
                            Department = adUserInfo.Department,
                            Position = adUserInfo.Title,
                            Email = adUserInfo.Email,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Employees.Add(employee);
                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = $"משתמש {username} נוסף בהצלחה. מנהל: {(isInManagementGroup ? "כן" : "לא")}";
                }
                else
                {
                    TempData["Error"] = $"שגיאה בהוספת משתמש: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"שגיאה: {ex.Message}";
            }

            return RedirectToAction(nameof(ImportUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportManagersFromGroup()
        {
            // Check if AD is enabled
            if (!_adSettings.Enabled || string.IsNullOrWhiteSpace(_adSettings.LdapPath) || _adSettings.LdapPath.Contains("yourdomain.com"))
            {
                TempData["Error"] = "אקטיב דירקטורי אינו מופעל או לא מוגדר. אנא השתמש ביצירת משתמשים ידנית.";
                return RedirectToAction(nameof(ImportUsers));
            }

            try
            {
                var usersFromGroup = await _adService.GetUsersFromGroupAsync(_adSettings.ManagementGroup);
                int added = 0;
                int updated = 0;
                int errors = 0;

                foreach (var adUserInfo in usersFromGroup)
                {
                    try
                    {
                        var existingUser = await _userManager.FindByNameAsync(adUserInfo.Username);
                        
                        if (existingUser == null)
                        {
                            // Create new user
                            var user = new ApplicationUser
                            {
                                UserName = adUserInfo.Username,
                                Email = adUserInfo.Email ?? $"{adUserInfo.Username}@example.com",
                                FullName = adUserInfo.FullName,
                                Department = adUserInfo.Department,
                                IsManager = true,
                                IsAdmin = false,
                                EmailConfirmed = true
                            };

                            var password = Guid.NewGuid().ToString() + "!Aa1";
                            var result = await _userManager.CreateAsync(user, password);
                            
                            if (result.Succeeded)
                            {
                                await _userManager.AddToRoleAsync(user, "Manager");
                                added++;
                            }
                            else
                            {
                                errors++;
                            }
                        }
                        else
                        {
                            // Update existing user to be manager
                            existingUser.IsManager = true;
                            existingUser.FullName = adUserInfo.FullName;
                            existingUser.Department = adUserInfo.Department;
                            if (!string.IsNullOrEmpty(adUserInfo.Email))
                            {
                                existingUser.Email = adUserInfo.Email;
                            }

                            await _userManager.UpdateAsync(existingUser);
                            
                            if (!await _userManager.IsInRoleAsync(existingUser, "Manager"))
                            {
                                await _userManager.AddToRoleAsync(existingUser, "Manager");
                            }
                            
                            updated++;
                        }
                    }
                    catch (Exception)
                    {
                        errors++;
                    }
                }

                TempData["Success"] = $"ייבוא הושלם: {added} נוספו, {updated} עודכנו, {errors} שגיאות";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"שגיאה בייבוא: {ex.Message}";
            }

            return RedirectToAction(nameof(ImportUsers));
        }
    }
}

