using Microsoft.AspNetCore.Identity;

namespace AuthorizationForm.Models
{
    public class SearchResultsViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<AuthorizationRequest> Requests { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();
        public List<ApplicationSystem> Systems { get; set; } = new();
        public List<ApplicationUser> Users { get; set; } = new();
        public List<FormTemplate> FormTemplates { get; set; } = new();
        public List<EmailTemplate> EmailTemplates { get; set; } = new();
        public List<RequestHistory> RequestHistories { get; set; } = new();
        
        public int TotalResults => Requests.Count + Employees.Count + Systems.Count + 
                                   Users.Count + FormTemplates.Count + EmailTemplates.Count + 
                                   RequestHistories.Count;
    }
}

