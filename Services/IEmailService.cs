namespace AuthorizationForm.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendAuthorizationRequestAsync(Models.AuthorizationRequest request);
        Task SendManagerApprovalRequestAsync(Models.AuthorizationRequest request);
        Task SendFinalApprovalRequestAsync(Models.AuthorizationRequest request);
        Task SendRequestStatusUpdateAsync(Models.AuthorizationRequest request);
    }
}

