using AuthorizationForm.Models;

namespace AuthorizationForm.Services
{
    public interface IAuthorizationService
    {
        Task AddRequestHistoryAsync(int requestId, RequestStatus previousStatus, RequestStatus newStatus, string userId, string? comments = null);
        Task<bool> CanUserCancelRequestAsync(AuthorizationRequest request, string userId);
        Task<bool> CanManagerCancelRequestAsync(AuthorizationRequest request, string managerId);
    }
}

