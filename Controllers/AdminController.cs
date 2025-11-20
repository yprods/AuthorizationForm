using AuthorizationForm.Data;
using AuthorizationForm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationForm.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
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
    }
}

