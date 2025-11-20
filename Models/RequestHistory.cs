using System.ComponentModel.DataAnnotations;

namespace AuthorizationForm.Models
{
    public class RequestHistory
    {
        public int Id { get; set; }
        
        [Required]
        public int RequestId { get; set; }
        public AuthorizationRequest? Request { get; set; }
        
        public RequestStatus PreviousStatus { get; set; }
        public RequestStatus NewStatus { get; set; }
        
        public string? ActionPerformedBy { get; set; }
        public string? ActionPerformedById { get; set; }
        
        public string? Comments { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

