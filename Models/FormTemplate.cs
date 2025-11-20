using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Models
{
    public class FormTemplate
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public string TemplateContent { get; set; } = string.Empty; // JSON or HTML
        
        public string? PdfTemplatePath { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser? CreatedBy { get; set; }
    }
}

