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
            // If AD is not enabled, return false (cannot validate)
            if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.LdapPath) || _settings.LdapPath.Contains("yourdomain.com"))
            {
                _logger.LogInformation($"AD is disabled or not configured - skipping credential validation for {username}");
                return false;
            }

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
            // If AD is not enabled, return null
            if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.LdapPath) || _settings.LdapPath.Contains("yourdomain.com"))
            {
                _logger.LogDebug($"AD is disabled or not configured - cannot get full name for {username}");
                return null;
            }

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
            // If AD is not enabled, return null
            if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.LdapPath) || _settings.LdapPath.Contains("yourdomain.com"))
            {
                _logger.LogDebug($"AD is disabled or not configured - cannot get email for {username}");
                return null;
            }

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

        public async Task<ADUserInfo?> GetUserInfoAsync(string username)
        {
            // If AD is not enabled, return null
            if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.LdapPath) || _settings.LdapPath.Contains("yourdomain.com"))
            {
                _logger.LogDebug($"AD is disabled or not configured - cannot get user info for {username}");
                return null;
            }

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
                    searcher.PropertiesToLoad.Add("mail");
                    searcher.PropertiesToLoad.Add("department");
                    searcher.PropertiesToLoad.Add("title");
                    searcher.PropertiesToLoad.Add("employeeID");
                    searcher.PropertiesToLoad.Add("sAMAccountName");

                    var result = searcher.FindOne();
                    if (result == null) return null;

                    var userInfo = new ADUserInfo
                    {
                        Username = result.Properties["sAMAccountName"]?[0]?.ToString() ?? username,
                        FullName = result.Properties["displayName"]?[0]?.ToString() 
                                   ?? result.Properties["cn"]?[0]?.ToString() 
                                   ?? username,
                        Email = result.Properties["mail"]?[0]?.ToString(),
                        Department = result.Properties["department"]?[0]?.ToString(),
                        Title = result.Properties["title"]?[0]?.ToString(),
                        EmployeeId = result.Properties["employeeID"]?[0]?.ToString()
                    };

                    return userInfo;
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to get user info for {username}");
                return null;
            }
        }

        public async Task<bool> IsUserInGroupAsync(string username, string groupName)
        {
            // If AD is not enabled, return false
            if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.LdapPath) || _settings.LdapPath.Contains("yourdomain.com"))
            {
                _logger.LogDebug($"AD is disabled or not configured - cannot check group membership for {username}");
                return false;
            }

            try
            {
                return await Task.Run(() =>
                {
                    using var entry = new DirectoryEntry(_settings.LdapPath);
                    using var searcher = new DirectorySearcher(entry)
                    {
                        Filter = $"(&(objectClass=user)(sAMAccountName={username}))"
                    };
                    searcher.PropertiesToLoad.Add("memberOf");

                    var result = searcher.FindOne();
                    if (result == null) return false;

                    var memberOf = result.Properties["memberOf"];
                    if (memberOf == null || memberOf.Count == 0) return false;

                    for (int i = 0; i < memberOf.Count; i++)
                    {
                        var groupDN = memberOf[i]?.ToString() ?? "";
                        if (groupDN.Contains(groupName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    return false;
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to check if user {username} is in group {groupName}");
                return false;
            }
        }

        public async Task<List<ADUserInfo>> GetUsersFromGroupAsync(string groupName)
        {
            var users = new List<ADUserInfo>();
            
            // If AD is not enabled, return empty list
            if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.LdapPath) || _settings.LdapPath.Contains("yourdomain.com"))
            {
                _logger.LogDebug($"AD is disabled or not configured - cannot get users from group {groupName}");
                return users;
            }

            try
            {
                return await Task.Run(() =>
                {
                    using var entry = new DirectoryEntry(_settings.LdapPath);
                    
                    // First, find the group
                    using var groupSearcher = new DirectorySearcher(entry)
                    {
                        Filter = $"(&(objectClass=group)(cn={groupName}))"
                    };
                    groupSearcher.PropertiesToLoad.Add("distinguishedName");
                    
                    var groupResult = groupSearcher.FindOne();
                    if (groupResult == null)
                    {
                        _logger.LogWarning($"Group {groupName} not found");
                        return users;
                    }

                    var groupDN = groupResult.Properties["distinguishedName"]?[0]?.ToString();
                    if (string.IsNullOrEmpty(groupDN)) return users;

                    // Find all users in the group
                    using var userSearcher = new DirectorySearcher(entry)
                    {
                        Filter = $"(&(objectClass=user)(memberOf={groupDN}))"
                    };
                    userSearcher.PropertiesToLoad.Add("sAMAccountName");
                    userSearcher.PropertiesToLoad.Add("displayName");
                    userSearcher.PropertiesToLoad.Add("cn");
                    userSearcher.PropertiesToLoad.Add("mail");
                    userSearcher.PropertiesToLoad.Add("department");
                    userSearcher.PropertiesToLoad.Add("title");
                    userSearcher.PropertiesToLoad.Add("employeeID");

                    var userResults = userSearcher.FindAll();
                    foreach (SearchResult userResult in userResults)
                    {
                        var userInfo = new ADUserInfo
                        {
                            Username = userResult.Properties["sAMAccountName"]?[0]?.ToString() ?? "",
                            FullName = userResult.Properties["displayName"]?[0]?.ToString() 
                                       ?? userResult.Properties["cn"]?[0]?.ToString() 
                                       ?? "",
                            Email = userResult.Properties["mail"]?[0]?.ToString(),
                            Department = userResult.Properties["department"]?[0]?.ToString(),
                            Title = userResult.Properties["title"]?[0]?.ToString(),
                            EmployeeId = userResult.Properties["employeeID"]?[0]?.ToString()
                        };
                        if (!string.IsNullOrEmpty(userInfo.Username))
                        {
                            users.Add(userInfo);
                        }
                    }

                    return users;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get users from group {groupName}");
                return users;
            }
        }

        public async Task<List<ADUserInfo>> SearchUsersAsync(string searchTerm, int maxResults = 20)
        {
            var users = new List<ADUserInfo>();
            
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            {
                _logger.LogDebug($"Search term too short: {searchTerm}");
                return users;
            }

            try
            {
                // If AD is not enabled, return empty list
                if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.LdapPath) || _settings.LdapPath.Contains("yourdomain.com"))
                {
                    _logger.LogInformation($"AD is disabled or not configured - skipping AD search (offline mode). This is normal if working without domain.");
                    // Don't throw exception - return empty list so UI doesn't break
                    // Application will continue with local database search only
                    return new List<ADUserInfo>();
                }

                var ldapPath = _settings?.LdapPath ?? "";
                _logger.LogInformation($"Searching AD users with term: {searchTerm}, LdapPath: {(string.IsNullOrWhiteSpace(ldapPath) ? "NOT CONFIGURED" : ldapPath)}");
                
                return await Task.Run(() =>
                {
                    try
                    {
                        // Escape special LDAP characters in search term (but keep wildcards for search)
                        var escapedTerm = searchTerm.Replace("(", "\\28").Replace(")", "\\29").Replace("\\", "\\5c");
                        // Don't escape * as we want wildcard search
                        
                        using var entry = new DirectoryEntry(_settings.LdapPath);
                        
                        // Try authenticated search first, fallback to anonymous
                        try
                        {
                            var native = entry.NativeObject; // Test connection
                        }
                        catch
                        {
                            _logger.LogWarning("Could not access AD with current credentials, trying anonymous");
                        }
                        
                        using var searcher = new DirectorySearcher(entry)
                        {
                            Filter = $"(&(objectClass=user)(objectCategory=person)(|(displayName=*{escapedTerm}*)(sAMAccountName=*{escapedTerm}*)(mail=*{escapedTerm}*)(cn=*{escapedTerm}*)(givenName=*{escapedTerm}*)(sn=*{escapedTerm}*)))",
                            SizeLimit = maxResults,
                            PageSize = maxResults
                        };
                        
                        searcher.PropertiesToLoad.Add("sAMAccountName");
                        searcher.PropertiesToLoad.Add("displayName");
                        searcher.PropertiesToLoad.Add("cn");
                        searcher.PropertiesToLoad.Add("mail");
                        searcher.PropertiesToLoad.Add("department");
                        searcher.PropertiesToLoad.Add("title");
                        searcher.PropertiesToLoad.Add("employeeID");
                        searcher.PropertiesToLoad.Add("givenName");
                        searcher.PropertiesToLoad.Add("sn");

                        _logger.LogDebug($"LDAP Filter: {searcher.Filter}");
                        
                        SearchResultCollection? results = null;
                        try
                        {
                            results = searcher.FindAll();
                            _logger.LogInformation($"Found {results.Count} AD users matching '{searchTerm}'");
                            
                            int count = 0;
                            
                            foreach (SearchResult result in results)
                            {
                                if (count >= maxResults) break;
                                
                                try
                                {
                                    var username = result.Properties["sAMAccountName"]?[0]?.ToString();
                                    if (string.IsNullOrEmpty(username)) continue;
                                    
                                    // Skip computer accounts and service accounts
                                    var userAccountControl = result.Properties["userAccountControl"]?[0]?.ToString();
                                    
                                    var userInfo = new ADUserInfo
                                    {
                                        Username = username,
                                        FullName = result.Properties["displayName"]?[0]?.ToString() 
                                                   ?? result.Properties["cn"]?[0]?.ToString()
                                                   ?? $"{result.Properties["givenName"]?[0]?.ToString()} {result.Properties["sn"]?[0]?.ToString()}".Trim()
                                                   ?? username,
                                        Email = result.Properties["mail"]?[0]?.ToString(),
                                        Department = result.Properties["department"]?[0]?.ToString(),
                                        Title = result.Properties["title"]?[0]?.ToString(),
                                        EmployeeId = result.Properties["employeeID"]?[0]?.ToString()
                                    };
                                    
                                    if (!string.IsNullOrEmpty(userInfo.Username))
                                    {
                                        users.Add(userInfo);
                                        count++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, $"Error processing search result: {ex.Message}");
                                }
                            }
                        }
                        finally
                        {
                            results?.Dispose();
                        }

                        _logger.LogInformation($"Returning {users.Count} users from AD search");
                        return users;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error in AD search Task.Run: {ex.Message}");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to search users with term: {searchTerm}, Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return users;
            }
        }
    }
}

