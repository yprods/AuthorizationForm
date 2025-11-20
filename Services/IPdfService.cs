using AuthorizationForm.Models;

namespace AuthorizationForm.Services
{
    public interface IPdfService
    {
        Task<string> GenerateAuthorizationRequestPdfAsync(AuthorizationRequest request);
        Task<string> GeneratePdfFromTemplateAsync(FormTemplate template, Dictionary<string, string> data);
    }
}

