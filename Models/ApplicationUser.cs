using Microsoft.AspNetCore.Identity;

namespace AuthorizationForm.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Department { get; set; }
        public bool IsManager { get; set; }
        public bool IsAdmin { get; set; }
        public string? ManagerId { get; set; }
        public ApplicationUser? Manager { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

