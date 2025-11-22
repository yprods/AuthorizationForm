using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "שדה שם משתמש הוא חובה")]
        [Display(Name = "שם משתמש")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "שם המשתמש חייב להכיל בין 3 ל-50 תווים")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "שדה אימייל הוא חובה")]
        [EmailAddress(ErrorMessage = "כתובת אימייל לא תקינה")]
        [Display(Name = "אימייל")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "שדה שם מלא הוא חובה")]
        [Display(Name = "שם מלא")]
        [StringLength(100, ErrorMessage = "השם המלא לא יכול לעלות על 100 תווים")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "שדה סיסמה הוא חובה")]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמא")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "הסיסמה חייבת להכיל לפחות 6 תווים")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "שדה אימות סיסמה הוא חובה")]
        [DataType(DataType.Password)]
        [Display(Name = "אימות סיסמה")]
        [Compare("Password", ErrorMessage = "הסיסמאות אינן תואמות")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

