using AuthorizationForm.Data;
using AuthorizationForm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AuthorizationForm.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ManagerController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            // Get all requests this manager needs to approve
            var pendingApprovalsQuery = _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .Include(r => r.FinalApprover)
                .Where(r => r.Status == RequestStatus.PendingManagerApproval);

            if (!isAdmin)
            {
                // Managers can only see requests assigned to them
                pendingApprovalsQuery = pendingApprovalsQuery.Where(r => r.ManagerId == currentUser.Id);
            }
            // Admins can see all pending approvals

            var pendingApprovals = await pendingApprovalsQuery
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .ToListAsync();

            // Get statistics
            var allRequestsQuery = _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .AsQueryable();

            if (!isAdmin)
            {
                // Managers can only see requests they manage or their own
                allRequestsQuery = allRequestsQuery.Where(r => r.ManagerId == currentUser.Id || r.UserId == currentUser.Id);
            }

            var totalRequests = await allRequestsQuery.CountAsync();
            var pendingCount = await allRequestsQuery.CountAsync(r => r.Status == RequestStatus.PendingManagerApproval);
            var approvedCount = await allRequestsQuery.CountAsync(r => r.Status == RequestStatus.Approved);
            var rejectedCount = await allRequestsQuery.CountAsync(r => r.Status == RequestStatus.Rejected);
            var pendingFinalApproval = await allRequestsQuery.CountAsync(r => r.Status == RequestStatus.PendingFinalApproval);

            // Get recent requests (last 10)
            var recentRequests = await allRequestsQuery
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Get team members (users managed by this manager)
            var teamMembers = new List<ApplicationUser>();
            if (!isAdmin)
            {
                teamMembers = await _userManager.Users
                    .Where(u => u.ManagerId == currentUser.Id)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
            }
            else
            {
                // Admins see all users
                teamMembers = await _userManager.Users
                    .OrderBy(u => u.FullName)
                    .Take(50)
                    .ToListAsync();
            }

            ViewBag.PendingApprovals = pendingApprovals;
            ViewBag.TotalRequests = totalRequests;
            ViewBag.PendingCount = pendingCount;
            ViewBag.ApprovedCount = approvedCount;
            ViewBag.RejectedCount = rejectedCount;
            ViewBag.PendingFinalApproval = pendingFinalApproval;
            ViewBag.RecentRequests = recentRequests;
            ViewBag.TeamMembers = teamMembers;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.CurrentUser = currentUser;

            return View();
        }

        // Quick approve action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickApprove(int requestId, bool approved, string? comments = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var request = await _context.AuthorizationRequests
                .Include(r => r.User)
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                return Json(new { success = false, message = "בקשה לא נמצאה" });
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (request.ManagerId != currentUser.Id && !isAdmin)
            {
                return Json(new { success = false, message = "אין לך הרשאה לאשר בקשה זו" });
            }

            if (request.Status != RequestStatus.PendingManagerApproval)
            {
                return Json(new { success = false, message = "הבקשה כבר לא ממתינה לאישור מנהל" });
            }

            try
            {

            if (approved)
            {
                request.Status = RequestStatus.PendingFinalApproval;
                request.ManagerApprovedAt = DateTime.UtcNow;
                request.ManagerApprovalSignature = currentUser.FullName ?? currentUser.UserName;
                
                // Add to history
                _context.RequestHistories.Add(new RequestHistory
                {
                    RequestId = request.Id,
                    PreviousStatus = RequestStatus.PendingManagerApproval,
                    NewStatus = RequestStatus.PendingFinalApproval,
                    ActionPerformedBy = currentUser.FullName ?? currentUser.UserName,
                    ActionPerformedById = currentUser.Id,
                    Comments = comments ?? "אושר על ידי מנהל"
                });

                _logger.LogInformation($"Manager {currentUser.UserName} approved request {requestId}");
            }
            else
            {
                request.Status = RequestStatus.Rejected;
                request.RejectionReason = comments ?? "נדחה על ידי מנהל";
                request.ManagerApprovedAt = DateTime.UtcNow;
                
                // Add to history
                _context.RequestHistories.Add(new RequestHistory
                {
                    RequestId = request.Id,
                    PreviousStatus = RequestStatus.PendingManagerApproval,
                    NewStatus = RequestStatus.Rejected,
                    ActionPerformedBy = currentUser.FullName ?? currentUser.UserName,
                    ActionPerformedById = currentUser.Id,
                    Comments = comments ?? "נדחה על ידי מנהל"
                });

                _logger.LogInformation($"Manager {currentUser.UserName} rejected request {requestId}");
            }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = approved ? "בקשה אושרה בהצלחה" : "בקשה נדחתה" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving/rejecting request {requestId}");
                return Json(new { success = false, message = "אירעה שגיאה בעת ביצוע הפעולה: " + ex.Message });
            }
        }
    }
}

