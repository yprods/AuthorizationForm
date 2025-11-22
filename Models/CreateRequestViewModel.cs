using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Models
{
    public class CreateRequestViewModel
    {
        [Required]
        [Display(Name = "רמת שירות")]
        public ServiceLevel ServiceLevel { get; set; }

        [Required]
        [Display(Name = "עובדים נבחרים")]
        public List<int> SelectedEmployeeIds { get; set; } = new();

        [Required]
        [Display(Name = "מערכות נבחרות")]
        public List<int> SelectedSystemIds { get; set; } = new();

        [Display(Name = "הערות")]
        public string? Comments { get; set; }

        [Required]
        [Display(Name = "מנהל אחראי")]
        public string ManagerId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "גילוי נאות ואישור")]
        public bool DisclosureAcknowledged { get; set; }

        // Optional fields for anonymous users (required if not authenticated)
        [Display(Name = "שם מלא")]
        public string? UserFullName { get; set; }

        [EmailAddress(ErrorMessage = "פורמט אימייל לא תקין")]
        [Display(Name = "אימייל")]
        public string? UserEmail { get; set; }
    }
}

