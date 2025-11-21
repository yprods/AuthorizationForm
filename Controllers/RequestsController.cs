using AuthorizationForm.Data;
using AuthorizationForm.Models;
using AuthorizationForm.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<RequestsController> _logger;

        public RequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IPdfService pdfService,
            Services.IAuthorizationService authorizationService,
            IActiveDirectoryService adService,
            ILogger<RequestsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _pdfService = pdfService;
            _authorizationService = authorizationService;
            _adService = adService;
            _logger = logger;
        }

        // Search AD Users - API endpoint for auto-complete
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

            try
            {
                _logger.LogInformation($"Calling AD service to search for users with term: {term}");
                var users = await _adService.SearchUsersAsync(term, maxResults);
                _logger.LogInformation($"AD service returned {users?.Count ?? 0} users");
                
                if (users == null)
                {
                    _logger.LogWarning("AD service returned null - returning empty list");
                    return Json(new List<object>());
                }
                
                var results = users.Select(u => new
                {
                    username = u.Username ?? "",
                    fullName = u.FullName ?? "",
                    email = u.Email ?? "",
                    department = u.Department ?? "",
                    title = u.Title ?? ""
                }).ToList();

                _logger.LogInformation($"Returning {results.Count} results to client");
                
                // Return proper JSON response
                Response.ContentType = "application/json; charset=utf-8";
                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching AD users - Term: {term}, Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                
                // Return error as JSON but don't crash - return empty list with warning in response
                Response.ContentType = "application/json; charset=utf-8";
                
                // Return empty list instead of error object - UI will show "לא נמצאו תוצאות"
                // Log error for admin review
                _logger.LogWarning($"Returning empty list due to AD search error: {ex.Message}");
                return Json(new List<object>());
            }
        }

        // GET: Requests
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager") || user.IsManager;

            IQueryable<AuthorizationRequest> requests;

            if (isAdmin)
            {
                requests = _context.AuthorizationRequests
                    .Include(r => r.User)
                    .Include(r => r.Manager)
                    .OrderByDescending(r => r.CreatedAt);
            }
            else if (isManager)
            {
                requests = _context.AuthorizationRequests
                    .Include(r => r.User)
                    .Include(r => r.Manager)
                    .Where(r => r.ManagerId == user.Id || r.Status == RequestStatus.PendingManagerApproval)
                    .OrderByDescending(r => r.CreatedAt);
            }
            else
            {
                requests = _context.AuthorizationRequests
                    .Include(r => r.User)
                    .Include(r => r.Manager)
                    .Where(r => r.UserId == user.Id)
                    .OrderByDescending(r => r.CreatedAt);
            }

            return View(await requests.ToListAsync());
        }

        // GET: Requests/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
            ViewBag.Systems = await _context.Systems.Where(s => s.IsActive).ToListAsync();
            
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            ViewBag.Managers = managers.Where(m => m.IsManager || m.IsAdmin).ToList();

            return View();
        }

        // POST: Requests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                await _authorizationService.AddRequestHistoryAsync(
                    request.Id, 
                    RequestStatus.Draft, 
                    RequestStatus.Draft, 
                    user.Id,
                    "בקשה נוצרה");

                // If disclosed, send to manager
                if (model.DisclosureAcknowledged)
                {
                    request.Status = RequestStatus.PendingManagerApproval;
                    request.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await _authorizationService.AddRequestHistoryAsync(
                        request.Id,
                        RequestStatus.Draft,
                        RequestStatus.PendingManagerApproval,
                        user.Id,
                        "בקשה נשלחה לאישור מנהל");

                    await _emailService.SendManagerApprovalRequestAsync(request);
                    await _emailService.SendAuthorizationRequestAsync(request);
                }

                return RedirectToAction(nameof(Details), new { id = request.Id });
            }

            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
            ViewBag.Systems = await _context.Systems.Where(s => s.IsActive).ToListAsync();
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            ViewBag.Managers = managers.Where(m => m.IsManager || m.IsAdmin).ToList();

            return View(model);
        }

        // GET: Requests/Details/5
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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // Check permissions
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                    var canView = isAdmin || 
                         request.UserId == currentUser.Id || 
                         (request.Manager != null && request.ManagerId == currentUser.Id);

            if (!canView) return Forbid();

            return View(request);
        }

        // GET: Requests/ManagerApprove/5
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ManagerApprove(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

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
            var request = await _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // Validate AD credentials
            var isValid = await _adService.ValidateCredentialsAsync(username, password);
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

                await _authorizationService.AddRequestHistoryAsync(
                    id, previousStatus, RequestStatus.PendingFinalApproval, currentUser.Id, "אושר על ידי מנהל");

                await _emailService.SendFinalApprovalRequestAsync(request);
            }
            else
            {
                request.Status = RequestStatus.Rejected;
                request.RejectionReason = "נדחה על ידי מנהל";
                request.UpdatedAt = DateTime.UtcNow;

                await _authorizationService.AddRequestHistoryAsync(
                    id, previousStatus, RequestStatus.Rejected, currentUser.Id, "נדחה על ידי מנהל");
            }

            await _context.SaveChangesAsync();
            await _emailService.SendRequestStatusUpdateAsync(request);

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Requests/FinalApprove/5
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> FinalApprove(int? id)
        {
            if (id == null) return NotFound();

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
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> FinalApprove(int id, bool approved, string? comments)
        {
            var request = await _context.AuthorizationRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

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
            var request = await _context.AuthorizationRequests.FindAsync(id);
            if (request == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var canCancel = await _authorizationService.CanUserCancelRequestAsync(request, currentUser.Id);
            if (!canCancel)
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
            var request = await _context.AuthorizationRequests.FindAsync(id);
            if (request == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var canCancel = await _authorizationService.CanManagerCancelRequestAsync(request, currentUser.Id);
            if (!canCancel && !await _userManager.IsInRoleAsync(currentUser, "Admin"))
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
            var request = await _context.AuthorizationRequests.FindAsync(id);
            if (request == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var canView = isAdmin || request.UserId == currentUser.Id || request.ManagerId == currentUser.Id;

            if (!canView) return Forbid();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(request.PdfPath);
            return File(fileBytes, "application/pdf", $"AuthorizationRequest_{request.Id}.pdf");
        }
    }
}

