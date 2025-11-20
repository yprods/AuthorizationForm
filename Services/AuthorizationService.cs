using AuthorizationForm.Data;
using AuthorizationForm.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthorizationForm.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthorizationService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task AddRequestHistoryAsync(int requestId, RequestStatus previousStatus, RequestStatus newStatus, string userId, string? comments = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var history = new RequestHistory
            {
                RequestId = requestId,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                ActionPerformedById = userId,
                ActionPerformedBy = user?.FullName ?? user?.UserName ?? "מערכת",
                Comments = comments
            };

            _context.RequestHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanUserCancelRequestAsync(AuthorizationRequest request, string userId)
        {
            if (request.UserId != userId)
                return false;

            // User can cancel if status is Draft or PendingManagerApproval
            return request.Status == RequestStatus.Draft 
                   || request.Status == RequestStatus.PendingManagerApproval;
        }

        public async Task<bool> CanManagerCancelRequestAsync(AuthorizationRequest request, string managerId)
        {
            if (request.ManagerId != managerId)
                return false;

            // Manager can cancel if status is PendingManagerApproval or PendingFinalApproval
            return request.Status == RequestStatus.PendingManagerApproval 
                   || request.Status == RequestStatus.PendingFinalApproval;
        }
    }
}

