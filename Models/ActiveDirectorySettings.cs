namespace AuthorizationForm.Models
{
    public class ActiveDirectorySettings
    {
        public bool Enabled { get; set; } = false;
        public string Domain { get; set; } = string.Empty;
        public string LdapPath { get; set; } = string.Empty;
        public string ManagementGroup { get; set; } = "ניהול";
    }
}

