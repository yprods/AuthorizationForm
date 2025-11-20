using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "אימייל")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמא")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "זכור אותי")]
        public bool RememberMe { get; set; }
    }
}

