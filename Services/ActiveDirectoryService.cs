using System.DirectoryServices;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using AuthorizationForm.Models;

namespace AuthorizationForm.Services
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly ActiveDirectorySettings _settings;
        private readonly ILogger<ActiveDirectoryService> _logger;

        public ActiveDirectoryService(IOptions<ActiveDirectorySettings> settings, ILogger<ActiveDirectoryService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var entry = new DirectoryEntry(_settings.LdapPath, username, password);
                    try
                    {
                        var nativeObject = entry.NativeObject;
                        return true;
                    }
                    finally
                    {
                        entry.Dispose();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to validate credentials for user {username}");
                return false;
            }
        }

        public async Task<string?> GetUserFullNameAsync(string username)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var entry = new DirectoryEntry(_settings.LdapPath);
                    using var searcher = new DirectorySearcher(entry)
                    {
                        Filter = $"(&(objectClass=user)(sAMAccountName={username}))"
                    };
                    searcher.PropertiesToLoad.Add("displayName");
                    searcher.PropertiesToLoad.Add("cn");

                    var result = searcher.FindOne();
                    return result?.Properties["displayName"]?[0]?.ToString() 
                           ?? result?.Properties["cn"]?[0]?.ToString();
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to get full name for user {username}");
                return null;
            }
        }

        public async Task<string?> GetUserEmailAsync(string username)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var entry = new DirectoryEntry(_settings.LdapPath);
                    using var searcher = new DirectorySearcher(entry)
                    {
                        Filter = $"(&(objectClass=user)(sAMAccountName={username}))"
                    };
                    searcher.PropertiesToLoad.Add("mail");

                    var result = searcher.FindOne();
                    return result?.Properties["mail"]?[0]?.ToString();
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to get email for user {username}");
                return null;
            }
        }
    }
}

