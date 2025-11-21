using AuthorizationForm.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AuthorizationForm.Services
{
    public class CachedActiveDirectoryService : IActiveDirectoryService
    {
        private readonly IActiveDirectoryService _adService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedActiveDirectoryService> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

        public CachedActiveDirectoryService(
            IActiveDirectoryService adService,
            IMemoryCache cache,
            ILogger<CachedActiveDirectoryService> logger)
        {
            _adService = adService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            // Don't cache credentials validation
            return await _adService.ValidateCredentialsAsync(username, password);
        }

        public async Task<string?> GetUserFullNameAsync(string username)
        {
            var cacheKey = $"ad_user_fullname_{username.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out string? cachedName))
            {
                return cachedName;
            }

            var result = await _adService.GetUserFullNameAsync(username);
            if (result != null)
            {
                _cache.Set(cacheKey, result, _cacheExpiration);
            }
            return result;
        }

        public async Task<string?> GetUserEmailAsync(string username)
        {
            var cacheKey = $"ad_user_email_{username.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out string? cachedEmail))
            {
                return cachedEmail;
            }

            var result = await _adService.GetUserEmailAsync(username);
            if (result != null)
            {
                _cache.Set(cacheKey, result, _cacheExpiration);
            }
            return result;
        }

        public async Task<ADUserInfo?> GetUserInfoAsync(string username)
        {
            var cacheKey = $"ad_user_info_{username.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out ADUserInfo? cachedInfo))
            {
                return cachedInfo;
            }

            var result = await _adService.GetUserInfoAsync(username);
            if (result != null)
            {
                _cache.Set(cacheKey, result, _cacheExpiration);
            }
            return result;
        }

        public async Task<bool> IsUserInGroupAsync(string username, string groupName)
        {
            var cacheKey = $"ad_user_group_{username.ToLower()}_{groupName.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            var result = await _adService.IsUserInGroupAsync(username, groupName);
            _cache.Set(cacheKey, result, _cacheExpiration);
            return result;
        }

        public async Task<List<ADUserInfo>> GetUsersFromGroupAsync(string groupName)
        {
            var cacheKey = $"ad_group_users_{groupName.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out List<ADUserInfo>? cachedUsers))
            {
                return cachedUsers ?? new List<ADUserInfo>();
            }

            var result = await _adService.GetUsersFromGroupAsync(groupName);
            _cache.Set(cacheKey, result, _cacheExpiration);
            return result;
        }

        public async Task<List<ADUserInfo>> SearchUsersAsync(string searchTerm, int maxResults = 20)
        {
            // Don't cache search results as they're dynamic
            return await _adService.SearchUsersAsync(searchTerm, maxResults);
        }
    }
}

