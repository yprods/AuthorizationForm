using AuthorizationForm.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace AuthorizationForm.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                throw;
            }
        }

        public async Task SendAuthorizationRequestAsync(AuthorizationRequest request)
        {
            var subject = "בקשת הרשאות חדשה - דורשת אישור";
            var body = $@"
                <html dir='rtl'>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; direction: rtl; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 15px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 10px; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>בקשת הרשאות חדשה</h2>
                        </div>
                        <div class='content'>
                            <p>שלום {request.User?.FullName ?? "משתמש"},</p>
                            <p>התקבלה בקשת הרשאות חדשה.</p>
                            <p><strong>מספר בקשה:</strong> {request.Id}</p>
                            <p><strong>רמת שירות:</strong> {GetServiceLevelName(request.ServiceLevel)}</p>
                            <p><strong>מערכות נבחרו:</strong> {request.SelectedSystems}</p>
                            <p><a href='{GenerateApprovalLink(request.Id)}'>לצפייה בבקשה</a></p>
                        </div>
                        <div class='footer'>
                            <p>זהו מייל אוטומטי, אנא אל תשיב עליו.</p>
                        </div>
                    </div>
                </body>
                </html>";

            if (!string.IsNullOrEmpty(request.User?.Email))
            {
                await SendEmailAsync(request.User.Email, subject, body);
            }
        }

        public async Task SendManagerApprovalRequestAsync(AuthorizationRequest request)
        {
            var subject = "בקשת אישור מנהל - בקשת הרשאות";
            var approvalLink = GenerateManagerApprovalLink(request.Id);

            var body = $@"
                <html dir='rtl'>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; direction: rtl; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 15px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .button {{ background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>אישור מנהל נדרש</h2>
                        </div>
                        <div class='content'>
                            <p>שלום {request.Manager?.FullName ?? "מנהל"},</p>
                            <p>התקבלה בקשת הרשאות הממתינה לאישורך.</p>
                            <p><strong>מספר בקשה:</strong> {request.Id}</p>
                            <p><strong>מבקש:</strong> {request.User?.FullName}</p>
                            <p><strong>מערכות:</strong> {request.SelectedSystems}</p>
                            <p style='text-align: center; margin: 30px 0;'>
                                <a href='{approvalLink}' class='button'>לחיצה לאישור</a>
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            if (!string.IsNullOrEmpty(request.Manager?.Email))
            {
                await SendEmailAsync(request.Manager.Email, subject, body);
            }
        }

        public async Task SendFinalApprovalRequestAsync(AuthorizationRequest request)
        {
            var subject = "בקשת אישור סופי - בקשת הרשאות";
            var approvalLink = GenerateFinalApprovalLink(request.Id);

            var body = $@"
                <html dir='rtl'>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; direction: rtl; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #FF9800; color: white; padding: 15px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .button {{ background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>אישור סופי נדרש</h2>
                        </div>
                        <div class='content'>
                            <p>שלום,</p>
                            <p>בקשת הרשאות אושרה על ידי המנהל וממתינה לאישור סופי.</p>
                            <p><strong>מספר בקשה:</strong> {request.Id}</p>
                            <p><strong>מבקש:</strong> {request.User?.FullName}</p>
                            <p style='text-align: center; margin: 30px 0;'>
                                <a href='{approvalLink}' class='button'>לחיצה לאישור/דחייה</a>
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            // Send to final approver if specified, otherwise use configured email
            var finalApproverEmail = request.FinalApprover?.Email ?? _emailSettings.SenderEmail;
            await SendEmailAsync(finalApproverEmail, subject, body);
        }

        public async Task SendRequestStatusUpdateAsync(AuthorizationRequest request)
        {
            var statusName = GetStatusName(request.Status);
            var subject = $"עדכון סטטוס בקשת הרשאות #{request.Id} - {statusName}";

            var body = $@"
                <html dir='rtl'>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; direction: rtl; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #9C27B0; color: white; padding: 15px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>עדכון סטטוס</h2>
                        </div>
                        <div class='content'>
                            <p>שלום {request.User?.FullName ?? "משתמש"},</p>
                            <p>סטטוס בקשת ההרשאה שלך עודכן:</p>
                            <p><strong>מספר בקשה:</strong> {request.Id}</p>
                            <p><strong>סטטוס חדש:</strong> {statusName}</p>
                            {(request.Status == RequestStatus.Rejected && !string.IsNullOrEmpty(request.RejectionReason) 
                                ? $"<p><strong>סיבת דחייה:</strong> {request.RejectionReason}</p>" 
                                : "")}
                        </div>
                    </div>
                </body>
                </html>";

            if (!string.IsNullOrEmpty(request.User?.Email))
            {
                await SendEmailAsync(request.User.Email, subject, body);
            }
        }

        private string GenerateApprovalLink(int requestId)
        {
            return $"https://yoursite.com/Requests/Details/{requestId}";
        }

        private string GenerateManagerApprovalLink(int requestId)
        {
            return $"https://yoursite.com/Requests/ManagerApprove/{requestId}";
        }

        private string GenerateFinalApprovalLink(int requestId)
        {
            return $"https://yoursite.com/Requests/FinalApprove/{requestId}";
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

