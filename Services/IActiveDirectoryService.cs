namespace AuthorizationForm.Services
{
    public interface IActiveDirectoryService
    {
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<string?> GetUserFullNameAsync(string username);
        Task<string?> GetUserEmailAsync(string username);
    }
}

