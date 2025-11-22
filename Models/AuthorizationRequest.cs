using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Models
{
    public enum ServiceLevel
    {
        UserLevel = 1,
        OtherUserLevel = 2,
        MultipleUsers = 3
    }

    public enum RequestStatus
    {
        Draft = 0,
        PendingManagerApproval = 1,
        PendingFinalApproval = 2,
        Approved = 3,
        Rejected = 4,
        CancelledByUser = 5,
        CancelledByManager = 6,
        ManagerChanged = 7
    }

    public class AuthorizationRequest
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        
        [Required]
        public ServiceLevel ServiceLevel { get; set; }
        
        [Required]
        public string SelectedEmployees { get; set; } = string.Empty; // JSON array
        
        [Required]
        public string SelectedSystems { get; set; } = string.Empty; // JSON array
        
        public string? Comments { get; set; }
        
        [Required]
        public string ManagerId { get; set; } = string.Empty;
        public ApplicationUser? Manager { get; set; }
        
        public string? FinalApproverId { get; set; }
        public ApplicationUser? FinalApprover { get; set; }
        
        public RequestStatus Status { get; set; } = RequestStatus.Draft;
        
        public DateTime? ManagerApprovedAt { get; set; }
        public string? ManagerApprovalSignature { get; set; }
        
        public DateTime? FinalApprovedAt { get; set; }
        public string? FinalApprovalDecision { get; set; } // Approved/Rejected
        public string? FinalApprovalComments { get; set; }
        
        public bool DisclosureAcknowledged { get; set; }
        public DateTime? DisclosureAcknowledgedAt { get; set; }
        
        public string? RejectionReason { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public string? ChangedByAdminId { get; set; }
        public ApplicationUser? ChangedByAdmin { get; set; }
        
        // For tracking manager changes
        public string? PreviousManagerId { get; set; }
        public DateTime? ManagerChangedAt { get; set; }
        
        // PDF path if generated
        public string? PdfPath { get; set; }
        
        // Reminder tracking
        public DateTime? LastReminderSentAt { get; set; }
        public int ReminderCount { get; set; } = 0;
        
        public ICollection<RequestHistory> History { get; set; } = new List<RequestHistory>();
    }
}

