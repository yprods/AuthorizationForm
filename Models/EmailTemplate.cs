using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Models
{
    public enum EmailTriggerType
    {
        RequestCreated = 1,              // כשבקשה נוצרת
        ManagerApprovalRequest = 2,      // כשמבקשים אישור מנהל
        ManagerApproved = 3,             // כשמנהל מאשר
        ManagerRejected = 4,             // כשמנהל דוחה
        FinalApprovalRequest = 5,        // כשמבקשים אישור סופי
        FinalApproved = 6,               // כשמאשרים סופית
        FinalRejected = 7,               // כשדוחים סופית
        RequestCancelledByUser = 8,      // כשמשתמש מבטל
        RequestCancelledByManager = 9,   // כשמנהל מבטל
        StatusChanged = 10               // כשהסטטוס משתנה
    }

    public class EmailTemplate
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public EmailTriggerType TriggerType { get; set; }
        
        [Required]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        public string Body { get; set; } = string.Empty; // HTML template
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser? CreatedBy { get; set; }
        
        // Recipients configuration
        public string RecipientType { get; set; } = "User"; // User, Manager, FinalApprover, Custom
        public string? CustomRecipients { get; set; } // Comma-separated emails for Custom type
    }
}

