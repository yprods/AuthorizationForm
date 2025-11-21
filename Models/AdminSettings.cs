namespace AuthorizationForm.Models
{
    public class AdminSettings
    {
        public List<AdminUser> AdminUsers { get; set; } = new List<AdminUser>();
    }

    public class AdminUser
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = "מנהל מערכת";
        public bool IsDomainUser { get; set; } = false;
    }
}

