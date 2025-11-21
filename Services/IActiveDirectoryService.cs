using AuthorizationForm.Models;

namespace AuthorizationForm.Services
{
    public interface IActiveDirectoryService
    {
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<string?> GetUserFullNameAsync(string username);
        Task<string?> GetUserEmailAsync(string username);
        Task<ADUserInfo?> GetUserInfoAsync(string username);
        Task<bool> IsUserInGroupAsync(string username, string groupName);
        Task<List<ADUserInfo>> GetUsersFromGroupAsync(string groupName);
        Task<List<ADUserInfo>> SearchUsersAsync(string searchTerm, int maxResults = 20);
    }

    public class ADUserInfo
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public string? EmployeeId { get; set; }
    }
}

