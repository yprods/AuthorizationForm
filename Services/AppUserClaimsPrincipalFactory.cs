using AuthorizationForm.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AuthorizationForm.Services
{
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            
            // Reload user from database to get latest data
            user = await UserManager.FindByIdAsync(user.Id);
            if (user == null) return identity;
            
            // Add FullName claim
            if (!string.IsNullOrEmpty(user.FullName))
            {
                // Remove existing FullName claim if present
                var existingFullName = identity.FindFirst("FullName");
                if (existingFullName != null)
                {
                    identity.RemoveClaim(existingFullName);
                }
                identity.AddClaim(new Claim("FullName", user.FullName));
            }
            
            // Ensure all roles are present - get fresh roles from database
            var roles = await UserManager.GetRolesAsync(user);
            
            // Remove all existing role claims
            var existingRoleClaims = identity.FindAll(ClaimTypes.Role).ToList();
            foreach (var claim in existingRoleClaims)
            {
                identity.RemoveClaim(claim);
            }
            
            // Add current roles from database
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            
            // Also check if user.IsAdmin is true but no Admin role assigned
            if (user.IsAdmin && !roles.Contains("Admin"))
            {
                // This should be handled elsewhere, but as a safety measure, add it here too
                identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
            }
            
            return identity;
        }
    }
}


