using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "שדה שם משתמש הוא חובה")]
        [Display(Name = "שם משתמש")]
        public string Email { get; set; } = string.Empty; // Can be username or email

        [Required(ErrorMessage = "שדה סיסמה הוא חובה")]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמא")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "זכור אותי")]
        public bool RememberMe { get; set; }
    }
}

