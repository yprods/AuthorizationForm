using AuthorizationForm.Data;
using AuthorizationForm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace AuthorizationForm.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new SearchResultsViewModel
                {
                    Query = "",
                    Requests = new List<AuthorizationRequest>(),
                    Employees = new List<Employee>(),
                    Systems = new List<ApplicationSystem>(),
                    Users = new List<ApplicationUser>(),
                    FormTemplates = new List<FormTemplate>(),
                    EmailTemplates = new List<EmailTemplate>(),
                    RequestHistories = new List<RequestHistory>()
                });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isManager = await _userManager.IsInRoleAsync(currentUser, "Manager");

            // Normalize query for case-insensitive search
            var searchQuery = query.Trim().ToLower();

            var results = new SearchResultsViewModel
            {
                Query = query,
                Requests = new List<AuthorizationRequest>(),
                Employees = new List<Employee>(),
                Systems = new List<ApplicationSystem>(),
                Users = new List<ApplicationUser>(),
                FormTemplates = new List<FormTemplate>(),
                EmailTemplates = new List<EmailTemplate>(),
                RequestHistories = new List<RequestHistory>()
            };

            // Search requests (with role-based filtering)
            var requestsQuery = _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .Include(r => r.FinalApprover)
                .AsQueryable();

            if (!isAdmin && !isManager)
            {
                // Regular users can only see their own requests
                requestsQuery = requestsQuery.Where(r => r.UserId == currentUser.Id);
            }
            else if (isManager && !isAdmin)
            {
                // Managers can see their own requests and requests they manage
                requestsQuery = requestsQuery.Where(r => r.UserId == currentUser.Id || r.ManagerId == currentUser.Id);
            }
            // Admins can see all requests

            results.Requests = await requestsQuery
                .Where(r => 
                    r.Id.ToString().Contains(searchQuery) ||
                    (r.User != null && (
                        (r.User.FullName != null && r.User.FullName.ToLower().Contains(searchQuery)) ||
                        (r.User.Email != null && r.User.Email.ToLower().Contains(searchQuery)) ||
                        (r.User.UserName != null && r.User.UserName.ToLower().Contains(searchQuery))
                    )) ||
                    (r.Manager != null && (
                        (r.Manager.FullName != null && r.Manager.FullName.ToLower().Contains(searchQuery)) ||
                        (r.Manager.Email != null && r.Manager.Email.ToLower().Contains(searchQuery))
                    )) ||
                    (r.FinalApprover != null && (
                        (r.FinalApprover.FullName != null && r.FinalApprover.FullName.ToLower().Contains(searchQuery)) ||
                        (r.FinalApprover.Email != null && r.FinalApprover.Email.ToLower().Contains(searchQuery))
                    )) ||
                    (r.SelectedSystems != null && r.SelectedSystems.ToLower().Contains(searchQuery)) ||
                    (r.SelectedEmployees != null && r.SelectedEmployees.ToLower().Contains(searchQuery)) ||
                    (r.Comments != null && r.Comments.ToLower().Contains(searchQuery)) ||
                    (r.RejectionReason != null && r.RejectionReason.ToLower().Contains(searchQuery)) ||
                    r.Status.ToString().ToLower().Contains(searchQuery)
                )
                .OrderByDescending(r => r.CreatedAt)
                .Take(50)
                .ToListAsync();

            // Search employees (only for Admin and Manager)
            if (isAdmin || isManager)
            {
                results.Employees = await _context.Employees
                    .Where(e => e.IsActive && (
                        (e.FirstName != null && e.FirstName.ToLower().Contains(searchQuery)) ||
                        (e.LastName != null && e.LastName.ToLower().Contains(searchQuery)) ||
                        (e.EmployeeId != null && e.EmployeeId.ToLower().Contains(searchQuery)) ||
                        (e.Email != null && e.Email.ToLower().Contains(searchQuery)) ||
                        (e.Department != null && e.Department.ToLower().Contains(searchQuery)) ||
                        (e.Position != null && e.Position.ToLower().Contains(searchQuery)) ||
                        (e.Phone != null && e.Phone.Contains(searchQuery))
                    ))
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Take(50)
                    .ToListAsync();
            }

            // Search systems (only for Admin)
            if (isAdmin)
            {
                results.Systems = await _context.Systems
                    .Where(s => s.IsActive && (
                        (s.Name != null && s.Name.ToLower().Contains(searchQuery)) ||
                        (s.Description != null && s.Description.ToLower().Contains(searchQuery)) ||
                        (s.Category != null && s.Category.ToLower().Contains(searchQuery))
                    ))
                    .OrderBy(s => s.Name)
                    .Take(50)
                    .ToListAsync();
            }

            // Search users (only for Admin)
            if (isAdmin)
            {
                results.Users = await _userManager.Users
                    .Where(u =>
                        (u.UserName != null && u.UserName.ToLower().Contains(searchQuery)) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchQuery)) ||
                        (u.FullName != null && u.FullName.ToLower().Contains(searchQuery)) ||
                        (u.Department != null && u.Department.ToLower().Contains(searchQuery))
                    )
                    .OrderBy(u => u.FullName)
                    .ThenBy(u => u.UserName)
                    .Take(50)
                    .ToListAsync();
            }

            // Search form templates (only for Admin)
            if (isAdmin)
            {
                results.FormTemplates = await _context.FormTemplates
                    .Include(t => t.CreatedBy)
                    .Where(t =>
                        (t.Name != null && t.Name.ToLower().Contains(searchQuery)) ||
                        (t.Description != null && t.Description.ToLower().Contains(searchQuery)) ||
                        (t.TemplateContent != null && t.TemplateContent.ToLower().Contains(searchQuery)) ||
                        (t.CreatedBy != null && (
                            (t.CreatedBy.FullName != null && t.CreatedBy.FullName.ToLower().Contains(searchQuery)) ||
                            (t.CreatedBy.UserName != null && t.CreatedBy.UserName.ToLower().Contains(searchQuery))
                        ))
                    )
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(50)
                    .ToListAsync();
            }

            // Search email templates (only for Admin)
            if (isAdmin)
            {
                results.EmailTemplates = await _context.EmailTemplates
                    .Include(t => t.CreatedBy)
                    .Where(t =>
                        (t.Name != null && t.Name.ToLower().Contains(searchQuery)) ||
                        (t.Description != null && t.Description.ToLower().Contains(searchQuery)) ||
                        (t.Subject != null && t.Subject.ToLower().Contains(searchQuery)) ||
                        (t.Body != null && t.Body.ToLower().Contains(searchQuery)) ||
                        t.TriggerType.ToString().ToLower().Contains(searchQuery) ||
                        (t.CreatedBy != null && (
                            (t.CreatedBy.FullName != null && t.CreatedBy.FullName.ToLower().Contains(searchQuery)) ||
                            (t.CreatedBy.UserName != null && t.CreatedBy.UserName.ToLower().Contains(searchQuery))
                        ))
                    )
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(50)
                    .ToListAsync();
            }

            // Search request history (only for Admin and Manager, with role-based filtering)
            var historyQuery = _context.RequestHistories
                .Include(h => h.Request)
                    .ThenInclude(r => r.User)
                .Include(h => h.Request)
                    .ThenInclude(r => r.Manager)
                .AsQueryable();

            if (!isAdmin && isManager)
            {
                // Managers can only see history of requests they manage
                historyQuery = historyQuery.Where(h => h.Request != null && 
                    (h.Request.UserId == currentUser.Id || h.Request.ManagerId == currentUser.Id));
            }
            else if (!isAdmin && !isManager)
            {
                // Regular users can only see history of their own requests
                historyQuery = historyQuery.Where(h => h.Request != null && h.Request.UserId == currentUser.Id);
            }
            // Admins can see all history

            results.RequestHistories = await historyQuery
                .Where(h =>
                    (h.ActionPerformedBy != null && h.ActionPerformedBy.ToLower().Contains(searchQuery)) ||
                    (h.Comments != null && h.Comments.ToLower().Contains(searchQuery)) ||
                    h.PreviousStatus.ToString().ToLower().Contains(searchQuery) ||
                    h.NewStatus.ToString().ToLower().Contains(searchQuery) ||
                    (h.Request != null && (
                        h.Request.Id.ToString().Contains(searchQuery) ||
                        (h.Request.User != null && (
                            (h.Request.User.FullName != null && h.Request.User.FullName.ToLower().Contains(searchQuery)) ||
                            (h.Request.User.Email != null && h.Request.User.Email.ToLower().Contains(searchQuery))
                        ))
                    ))
                )
                .OrderByDescending(h => h.CreatedAt)
                .Take(50)
                .ToListAsync();

            return View(results);
        }
    }
}
