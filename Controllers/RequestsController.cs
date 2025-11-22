using AuthorizationForm.Data;
using AuthorizationForm.Models;
using AuthorizationForm.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AuthorizationForm.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;
        private readonly Services.IAuthorizationService _authorizationService;
        private readonly IActiveDirectoryService _adService;
        private readonly Microsoft.Extensions.Options.IOptions<ActiveDirectorySettings> _adSettings;
        private readonly ILogger<RequestsController> _logger;

        public RequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IPdfService pdfService,
            Services.IAuthorizationService authorizationService,
            IActiveDirectoryService adService,
            Microsoft.Extensions.Options.IOptions<ActiveDirectorySettings> adSettings,
            ILogger<RequestsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _pdfService = pdfService;
            _authorizationService = authorizationService;
            _adService = adService;
            _adSettings = adSettings;
            _logger = logger;
        }

        // Search Users (Local DB + AD) - API endpoint for auto-complete
        // Works offline - searches local database first, then AD if available
        [HttpGet]
        [Route("Requests/SearchAdUsers")]
        [AllowAnonymous] // Allow anonymous for testing, can be restricted later
        public async Task<IActionResult> SearchAdUsers(string term, int maxResults = 20)
        {
            _logger.LogInformation($"SearchAdUsers API called with term: '{term}', maxResults: {maxResults}");
            
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                _logger.LogDebug("Search term too short, returning empty results");
                return Json(new List<object>());
            }

            var allResults = new List<object>();
            
            // Step 1: Search in local database FIRST (always works, even offline)
            try
            {
                _logger.LogInformation($"Searching local database for users with term: {term}");
                var localUsers = await _context.Users
                    .Where(u => 
                        (u.UserName != null && u.UserName.Contains(term)) ||
                        (u.FullName != null && u.FullName.Contains(term)) ||
                        (u.Email != null && u.Email.Contains(term)))
                    .Where(u => u.IsManager || u.IsAdmin) // Only managers and admins
                    .Take(maxResults)
                    .ToListAsync();

                var localResults = localUsers.Select(u => new
                {
                    username = u.UserName ?? "",
                    fullName = u.FullName ?? u.UserName ?? "",
                    email = u.Email ?? "",
                    department = u.Department ?? "",
                    title = "",
                    isLocal = true,
                    userId = u.Id
                }).ToList();

                allResults.AddRange(localResults);
                _logger.LogInformation($"Found {localResults.Count} users in local database");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error searching local database: {ex.Message}");
                // Continue - try AD anyway
            }

            // Step 2: Try to search in Active Directory (if available and online)
            // This is optional - if AD fails, we still return local DB results
            try
            {
                _logger.LogInformation($"Attempting AD search for users with term: {term}");
                var adUsers = await _adService.SearchUsersAsync(term, maxResults);
                _logger.LogInformation($"AD service returned {adUsers?.Count ?? 0} users");
                
                if (adUsers != null && adUsers.Any())
                {
                    // Filter out users that are already in local database (avoid duplicates)
                    var existingUsernames = allResults.Select(r => 
                    {
                        var obj = r as dynamic;
                        return obj?.username?.ToString().ToLower() ?? "";
                    }).Where(u => !string.IsNullOrEmpty(u)).ToHashSet();
                    
                    var adResults = adUsers
                        .Where(ad => !string.IsNullOrEmpty(ad.Username) && !existingUsernames.Contains(ad.Username.ToLower()))
                        .Take(maxResults - allResults.Count)
                        .Select(u => new
                        {
                            username = u.Username ?? "",
                            fullName = u.FullName ?? "",
                            email = u.Email ?? "",
                            department = u.Department ?? "",
                            title = u.Title ?? "",
                            isLocal = false,
                            userId = (string?)null
                        }).ToList();

                    allResults.AddRange(adResults);
                    _logger.LogInformation($"Added {adResults.Count} users from AD");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"AD search failed (may be offline or not configured): {ex.Message}. Continuing with local database results only.");
                // Continue - we already have local database results, so we're good!
            }

            _logger.LogInformation($"Returning {allResults.Count} total results to client ({allResults.Count(r => (r as dynamic)?.isLocal == true)} local + {allResults.Count(r => (r as dynamic)?.isLocal == false)} AD)");
            
            // Return proper JSON response
            Response.ContentType = "application/json; charset=utf-8";
            return Json(allResults.Take(maxResults).ToList());
        }

        // GET: Requests
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isManager = await _userManager.IsInRoleAsync(currentUser, "Manager");

            var requestsQuery = _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
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

            return View(await requestsQuery.OrderByDescending(r => r.CreatedAt).ToListAsync());
        }

        // GET: Requests/Create - Allow anonymous access so users can fill forms
        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
            ViewBag.Systems = await _context.Systems.Where(s => s.IsActive).ToListAsync();
            
            // Get managers - if no managers exist, return empty list (form will still work)
            try
            {
                var managers = await _userManager.GetUsersInRoleAsync("Manager");
                ViewBag.Managers = managers?.Where(m => m.IsManager || m.IsAdmin).ToList() ?? new List<ApplicationUser>();
            }
            catch
            {
                ViewBag.Managers = new List<ApplicationUser>();
            }

            return View();
        }

        // POST: Requests/Create - Allow anonymous access so users can submit forms
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create(CreateRequestViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            // Allow anonymous users to create requests - create a user if needed
            ApplicationUser? user = currentUser;
            if (user == null)
            {
                // Require email for anonymous users
                if (string.IsNullOrWhiteSpace(model.UserEmail))
                {
                    ModelState.AddModelError(nameof(model.UserEmail), "שדה אימייל הוא חובה למשתמשים לא מחוברים");
                    ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
                    ViewBag.Systems = await _context.Systems.Where(s => s.IsActive).ToListAsync();
                    try
                    {
                        var managers = await _userManager.GetUsersInRoleAsync("Manager");
                        ViewBag.Managers = managers?.Where(m => m.IsManager || m.IsAdmin).ToList() ?? new List<ApplicationUser>();
                    }
                    catch
                    {
                        ViewBag.Managers = new List<ApplicationUser>();
                    }
                    return View(model);
                }
                else
                {
                    // Try to find or create a user based on email
                    user = await _userManager.FindByEmailAsync(model.UserEmail);
                    if (user == null)
                    {
                        // Create a new user from the form data
                        user = new ApplicationUser
                        {
                            UserName = model.UserEmail,
                            Email = model.UserEmail,
                            FullName = model.UserFullName ?? model.UserEmail.Split('@')[0],
                            EmailConfirmed = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        var createResult = await _userManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            foreach (var error in createResult.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
                            ViewBag.Systems = await _context.Systems.Where(s => s.IsActive).ToListAsync();
                            try
                            {
                                var managers = await _userManager.GetUsersInRoleAsync("Manager");
                                ViewBag.Managers = managers?.Where(m => m.IsManager || m.IsAdmin).ToList() ?? new List<ApplicationUser>();
                            }
                            catch
                            {
                                ViewBag.Managers = new List<ApplicationUser>();
                            }
                            return View(model);
                        }
                        else
                        {
                            // Assign User role
                            try
                            {
                                await _userManager.AddToRoleAsync(user, "User");
                            }
                            catch
                            {
                                // Role might not exist yet, continue anyway
                            }
                        }
                    }
                }
            }

            if (!ModelState.IsValid || user == null)
            {
                ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
                ViewBag.Systems = await _context.Systems.Where(s => s.IsActive).ToListAsync();
                try
                {
                    var managers = await _userManager.GetUsersInRoleAsync("Manager");
                    ViewBag.Managers = managers?.Where(m => m.IsManager || m.IsAdmin).ToList() ?? new List<ApplicationUser>();
                }
                catch
                {
                    ViewBag.Managers = new List<ApplicationUser>();
                }
                return View(model);
            }

            if (ModelState.IsValid && user != null)
            {
                var request = new AuthorizationRequest
                {
                    UserId = user.Id,
                    ServiceLevel = model.ServiceLevel,
                    SelectedEmployees = JsonSerializer.Serialize(model.SelectedEmployeeIds),
                    SelectedSystems = JsonSerializer.Serialize(model.SelectedSystemIds),
                    Comments = model.Comments,
                    ManagerId = model.ManagerId,
                    Status = RequestStatus.Draft,
                    DisclosureAcknowledged = model.DisclosureAcknowledged,
                    DisclosureAcknowledgedAt = model.DisclosureAcknowledged ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Add(request);
                await _context.SaveChangesAsync();

                // Add history
                try
                {
                    await _authorizationService.AddRequestHistoryAsync(
                        request.Id, 
                        RequestStatus.Draft, 
                        RequestStatus.Draft, 
                        user.Id,
                        "בקשה נוצרה");
                }
                catch
                {
                    // If history fails, continue anyway
                }

                // If disclosed, send to manager
                if (model.DisclosureAcknowledged)
                {
                    request.Status = RequestStatus.PendingManagerApproval;
                    request.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    try
                    {
                        await _authorizationService.AddRequestHistoryAsync(
                            request.Id,
                            RequestStatus.Draft,
                            RequestStatus.PendingManagerApproval,
                            user.Id,
                            "בקשה נשלחה לאישור מנהל");

                        await _emailService.SendManagerApprovalRequestAsync(request);
                        await _emailService.SendAuthorizationRequestAsync(request);
                    }
                    catch
                    {
                        // If email fails, continue anyway
                    }
                }

                return RedirectToAction(nameof(Details), new { id = request.Id });
            }

            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
            ViewBag.Systems = await _context.Systems.Where(s => s.IsActive).ToListAsync();
            try
            {
                var managers = await _userManager.GetUsersInRoleAsync("Manager");
                ViewBag.Managers = managers?.Where(m => m.IsManager || m.IsAdmin).ToList() ?? new List<ApplicationUser>();
            }
            catch
            {
                ViewBag.Managers = new List<ApplicationUser>();
            }

            return View(model);
        }

        // GET: Requests/Details/5 - Allow anonymous access
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .Include(r => r.FinalApprover)
                .Include(r => r.History)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return NotFound();

            // No authorization checks - allow all
            return View(request);
        }

        // GET: Requests/ManagerApprove/5
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ManagerApprove(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var request = await _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return NotFound();

            // Authorization check - only manager of the request or admin can approve
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (request.ManagerId != currentUser.Id && !isAdmin)
            {
                return Forbid();
            }

            return View(request);
        }

        // POST: Requests/ManagerApprove
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ManagerApprove(int id, string username, string password, bool approved)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var request = await _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            // Authorization check - only manager of the request or admin can approve
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (request.ManagerId != currentUser.Id && !isAdmin)
            {
                return Forbid();
            }

            // Validate credentials - try AD first if enabled, otherwise use local database
            bool isValid = false;
            if (_adSettings.Value?.Enabled == true && 
                !string.IsNullOrWhiteSpace(_adSettings.Value?.LdapPath) &&
                !_adSettings.Value.LdapPath.Contains("yourdomain.com"))
            {
                // Try AD validation
                isValid = await _adService.ValidateCredentialsAsync(username, password);
                _logger.LogInformation($"AD validation result for {username}: {isValid}");
            }
            
            // If AD validation failed or AD is disabled, try local database validation
            if (!isValid)
            {
                var userToValidate = await _userManager.FindByNameAsync(username);
                if (userToValidate != null)
                {
                    isValid = await _userManager.CheckPasswordAsync(userToValidate, password);
                    _logger.LogInformation($"Local database validation result for {username}: {isValid}");
                }
                else
                {
                    _logger.LogWarning($"User {username} not found in local database");
                }
            }
            
            if (!isValid)
            {
                ModelState.AddModelError("", "שם משתמש או סיסמה לא תקינים");
                return View(request);
            }

            var previousStatus = request.Status;

            if (approved)
            {
                request.Status = RequestStatus.PendingFinalApproval;
                request.ManagerApprovedAt = DateTime.UtcNow;
                request.ManagerApprovalSignature = username;
                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                // Reload with related data for email
                request = await _context.AuthorizationRequests
                    .Include(r => r.User)
                    .Include(r => r.Manager)
                    .Include(r => r.FinalApprover)
                    .FirstOrDefaultAsync(r => r.Id == id);

                await _authorizationService.AddRequestHistoryAsync(
                    id, previousStatus, RequestStatus.PendingFinalApproval, currentUser.Id, "אושר על ידי מנהל");

                await _emailService.SendFinalApprovalRequestAsync(request);
                await _emailService.SendRequestStatusUpdateAsync(request);
            }
            else
            {
                request.Status = RequestStatus.Rejected;
                request.RejectionReason = "נדחה על ידי מנהל";
                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                // Reload with related data for email
                request = await _context.AuthorizationRequests
                    .Include(r => r.User)
                    .Include(r => r.Manager)
                    .Include(r => r.FinalApprover)
                    .FirstOrDefaultAsync(r => r.Id == id);

                await _authorizationService.AddRequestHistoryAsync(
                    id, previousStatus, RequestStatus.Rejected, currentUser.Id, "נדחה על ידי מנהל");

                await _emailService.SendRequestStatusUpdateAsync(request);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Requests/FinalApprove/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FinalApprove(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var request = await _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return NotFound();

            return View(request);
        }

        // POST: Requests/FinalApprove
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FinalApprove(int id, bool approved, string? comments)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var request = await _context.AuthorizationRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            var previousStatus = request.Status;

            if (approved)
            {
                request.Status = RequestStatus.Approved;
                request.FinalApprovedAt = DateTime.UtcNow;
                request.FinalApproverId = currentUser.Id;
                request.FinalApprovalDecision = "Approved";
                request.FinalApprovalComments = comments;
                request.UpdatedAt = DateTime.UtcNow;

                // Generate PDF
                try
                {
                    request.PdfPath = await _pdfService.GenerateAuthorizationRequestPdfAsync(request);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // Log error but continue - PDF generation failed but request is still approved
                }

                await _authorizationService.AddRequestHistoryAsync(
                    id, previousStatus, RequestStatus.Approved, currentUser.Id, "אושר סופית");

                await _emailService.SendRequestStatusUpdateAsync(request);
            }
            else
            {
                request.Status = RequestStatus.Rejected;
                request.FinalApprovalDecision = "Rejected";
                request.FinalApprovalComments = comments;
                request.RejectionReason = comments ?? "נדחה סופית";
                request.UpdatedAt = DateTime.UtcNow;

                await _authorizationService.AddRequestHistoryAsync(
                    id, previousStatus, RequestStatus.Rejected, currentUser.Id, $"נדחה סופית: {comments}");

                await _emailService.SendRequestStatusUpdateAsync(request);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Requests/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var request = await _context.AuthorizationRequests.FindAsync(id);
            if (request == null) return NotFound();

            // Only the user who created the request can cancel it (unless admin)
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (request.UserId != currentUser.Id && !isAdmin)
            {
                return Forbid();
            }

            var previousStatus = request.Status;
            request.Status = RequestStatus.CancelledByUser;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _authorizationService.AddRequestHistoryAsync(
                id, previousStatus, RequestStatus.CancelledByUser, currentUser.Id, "בוטל על ידי משתמש");

            await _emailService.SendRequestStatusUpdateAsync(request);

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Requests/ManagerCancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ManagerCancel(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var request = await _context.AuthorizationRequests.FindAsync(id);
            if (request == null) return NotFound();

            // Authorization check - only manager of the request or admin can cancel
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (request.ManagerId != currentUser.Id && !isAdmin)
            {
                return Forbid();
            }

            var previousStatus = request.Status;
            request.Status = RequestStatus.CancelledByManager;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _authorizationService.AddRequestHistoryAsync(
                id, previousStatus, RequestStatus.CancelledByManager, currentUser.Id, "בוטל על ידי מנהל");

            await _emailService.SendRequestStatusUpdateAsync(request);

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Requests/ChangeManager/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeManager(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var request = await _context.AuthorizationRequests
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return NotFound();

            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            ViewBag.Managers = managers.Where(m => m.IsManager || m.IsAdmin).ToList();

            return View(request);
        }

        // POST: Requests/ChangeManager
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeManager(int id, string newManagerId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var request = await _context.AuthorizationRequests.FindAsync(id);
            if (request == null) return NotFound();

            var previousManagerId = request.ManagerId;
            request.PreviousManagerId = previousManagerId;
            request.ManagerId = newManagerId;
            request.Status = RequestStatus.ManagerChanged;
            request.ChangedByAdminId = currentUser.Id;
            request.ManagerChangedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _authorizationService.AddRequestHistoryAsync(
                id, RequestStatus.PendingManagerApproval, RequestStatus.ManagerChanged, 
                currentUser.Id, $"מנהל שונה מ-{previousManagerId} ל-{newManagerId}");

            // Send email to new manager
            await _emailService.SendManagerApprovalRequestAsync(request);

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Requests/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var request = await _context.AuthorizationRequests.FindAsync(id);
            if (request == null || string.IsNullOrEmpty(request.PdfPath) || !System.IO.File.Exists(request.PdfPath))
            {
                return NotFound();
            }

            // No authorization checks - allow all to view PDF

            var fileBytes = await System.IO.File.ReadAllBytesAsync(request.PdfPath);
            return File(fileBytes, "application/pdf", $"AuthorizationRequest_{request.Id}.pdf");
        }
    }
}

