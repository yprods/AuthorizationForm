using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Models
{
    public class SetupViewModel
    {
        [Required(ErrorMessage = "שדה שם משתמש הוא חובה")]
        [Display(Name = "שם משתמש")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "שדה אימייל הוא חובה")]
        [EmailAddress(ErrorMessage = "פורמט אימייל לא תקין")]
        [Display(Name = "אימייל")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "שם מלא")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "שדה סיסמה הוא חובה")]
        [StringLength(100, ErrorMessage = "הסיסמה חייבת להיות לפחות {2} תווים", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמא")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "אימות סיסמא")]
        [Compare("Password", ErrorMessage = "הסיסמה ואימות הסיסמה אינם תואמים.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

