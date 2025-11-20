using AuthorizationForm.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;

namespace AuthorizationForm.Services
{
    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;
        private readonly string _pdfOutputPath;

        public PdfService(ILogger<PdfService> logger, Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment)
        {
            _logger = logger;
            _pdfOutputPath = Path.Combine(environment.ContentRootPath, "Pdfs");
            if (!Directory.Exists(_pdfOutputPath))
            {
                Directory.CreateDirectory(_pdfOutputPath);
            }
        }

        public async Task<string> GenerateAuthorizationRequestPdfAsync(AuthorizationRequest request)
        {
            try
            {
                var fileName = $"AuthorizationRequest_{request.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                var filePath = Path.Combine(_pdfOutputPath, fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                var document = new Document(PageSize.A4, 50, 50, 50, 50);

                // Use BaseFont for Hebrew support
                var baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                var font = new Font(baseFont, 12);
                var titleFont = new Font(baseFont, 18, Font.BOLD);
                var headerFont = new Font(baseFont, 14, Font.BOLD);

                var writer = PdfWriter.GetInstance(document, fileStream);
                document.Open();

                // Title
                var title = new Paragraph("טופס בקשת הרשאות", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(title);

                // Request details
                document.Add(new Paragraph($"מספר בקשה: {request.Id}", font));
                document.Add(new Paragraph($"תאריך יצירה: {request.CreatedAt:dd/MM/yyyy HH:mm}", font));
                document.Add(new Paragraph($"מבקש: {request.User?.FullName ?? "לא זמין"}", font));
                document.Add(new Paragraph($"רמת שירות: {GetServiceLevelName(request.ServiceLevel)}", font));
                document.Add(new Paragraph($"מערכות נבחרו: {request.SelectedSystems}", font));
                document.Add(new Paragraph($"מנהל אחראי: {request.Manager?.FullName ?? "לא זמין"}", font));

                if (!string.IsNullOrEmpty(request.Comments))
                {
                    document.Add(new Paragraph($"הערות: {request.Comments}", font));
                }

                document.Add(new Paragraph($"סטטוס: {GetStatusName(request.Status)}", font));

                // Signature section
                document.Add(new Paragraph("\n\n\n", font));
                document.Add(new Paragraph("_________________________________", font));
                document.Add(new Paragraph("חתימת מבקש", font));

                if (request.Status == RequestStatus.Approved)
                {
                    document.Add(new Paragraph("\n\n", font));
                    document.Add(new Paragraph("_________________________________", font));
                    document.Add(new Paragraph("חתימת מנהל", font));
                }

                document.Close();
                writer.Close();

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF");
                throw;
            }
        }

        public async Task<string> GeneratePdfFromTemplateAsync(FormTemplate template, Dictionary<string, string> data)
        {
            try
            {
                var fileName = $"Template_{template.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                var filePath = Path.Combine(_pdfOutputPath, fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                var document = new Document(PageSize.A4);

                var baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                var font = new Font(baseFont, 12);

                var writer = PdfWriter.GetInstance(document, fileStream);
                document.Open();

                // Replace placeholders in template
                var content = template.TemplateContent;
                foreach (var kvp in data)
                {
                    content = content.Replace($"{{{kvp.Key}}}", kvp.Value);
                }

                var paragraph = new Paragraph(content, font);
                document.Add(paragraph);

                document.Close();
                writer.Close();

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF from template");
                throw;
            }
        }

        private string GetServiceLevelName(ServiceLevel level)
        {
            return level switch
            {
                ServiceLevel.UserLevel => "רמת משתמש",
                ServiceLevel.OtherUserLevel => "רמת משתמש אחר",
                ServiceLevel.MultipleUsers => "ריבוי משתמשים",
                _ => "לא ידוע"
            };
        }

        private string GetStatusName(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Draft => "טיוטה",
                RequestStatus.PendingManagerApproval => "ממתין לאישור מנהל",
                RequestStatus.PendingFinalApproval => "ממתין לאישור סופי",
                RequestStatus.Approved => "אושר",
                RequestStatus.Rejected => "נדחה",
                RequestStatus.CancelledByUser => "בוטל על ידי משתמש",
                RequestStatus.CancelledByManager => "בוטל על ידי מנהל",
                RequestStatus.ManagerChanged => "מנהל שונה",
                _ => "לא ידוע"
            };
        }
    }
}

