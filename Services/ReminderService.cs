using AuthorizationForm.Data;
using AuthorizationForm.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AuthorizationForm.Services
{
    public class ReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderService> _logger;
        private readonly TimeSpan _checkInterval;
        private readonly TimeSpan _reminderInterval;

        public ReminderService(
            IServiceProvider serviceProvider,
            ILogger<ReminderService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            // Check if reminders are enabled
            var enabled = configuration.GetValue<bool>("ReminderSettings:Enabled", true);
            
            if (!enabled)
            {
                _logger.LogInformation("ReminderService is disabled in configuration");
                _checkInterval = TimeSpan.FromHours(24); // Long interval if disabled
                _reminderInterval = TimeSpan.FromHours(24);
                return;
            }
            
            // Default: check every 6 hours, send reminder if pending for more than 24 hours
            var checkHours = configuration.GetValue<int>("ReminderSettings:CheckIntervalHours", 6);
            var reminderHours = configuration.GetValue<int>("ReminderSettings:ReminderIntervalHours", 24);
            
            _checkInterval = TimeSpan.FromHours(checkHours);
            _reminderInterval = TimeSpan.FromHours(reminderHours);
            
            _logger.LogInformation($"ReminderService initialized: Check every {checkHours} hours, Remind after {reminderHours} hours");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReminderService started");

            // Wait a bit before first check to let the application fully start
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ReminderService");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("ReminderService stopped");
        }

        private async Task CheckAndSendRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Check if reminders are enabled
            var enabled = configuration.GetValue<bool>("ReminderSettings:Enabled", true);
            if (!enabled)
            {
                _logger.LogDebug("ReminderService is disabled, skipping check");
                return;
            }

            try
            {
                var now = DateTime.UtcNow;
                var cutoffTime = now - _reminderInterval;

                // Find requests pending manager approval
                var pendingRequests = await context.AuthorizationRequests
                    .Include(r => r.User)
                    .Include(r => r.Manager)
                    .Where(r => r.Status == RequestStatus.PendingManagerApproval)
                    .ToListAsync();

                _logger.LogInformation($"Found {pendingRequests.Count} pending manager approvals");

                int remindersSent = 0;

                foreach (var request in pendingRequests)
                {
                    try
                    {
                        // Check if enough time has passed since last reminder (or since request was created)
                        var timeSinceLastReminder = request.LastReminderSentAt.HasValue
                            ? now - request.LastReminderSentAt.Value
                            : now - request.CreatedAt;

                        if (timeSinceLastReminder >= _reminderInterval)
                        {
                            // Send reminder
                            if (request.Manager != null && !string.IsNullOrEmpty(request.Manager.Email))
                            {
                                await SendReminderEmailAsync(emailService, request);
                                
                                // Update reminder tracking
                                request.LastReminderSentAt = now;
                                request.ReminderCount++;
                                
                                context.Update(request);
                                remindersSent++;
                                
                                _logger.LogInformation($"Sent reminder #{request.ReminderCount} for request {request.Id} to manager {request.Manager.Email}");
                            }
                            else
                            {
                                _logger.LogWarning($"Request {request.Id} has no manager email, cannot send reminder");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending reminder for request {request.Id}");
                    }
                }

                if (remindersSent > 0)
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Sent {remindersSent} reminder emails");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for reminders");
            }
        }

        private async Task SendReminderEmailAsync(IEmailService emailService, AuthorizationRequest request)
        {
            var manager = request.Manager;
            var requester = request.User;
            
            if (manager == null || string.IsNullOrEmpty(manager.Email))
            {
                _logger.LogWarning($"Cannot send reminder: Manager email is missing for request {request.Id}");
                return;
            }

            var daysPending = (DateTime.UtcNow - request.CreatedAt).Days;
            var hoursPending = (DateTime.UtcNow - request.CreatedAt).Hours;
            
            var subject = $"תזכורת: בקשה #{request.Id} ממתינה לאישורך";
            
            var body = $@"
<!DOCTYPE html>
<html dir='rtl' lang='he'>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; direction: rtl; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ff9800; color: white; padding: 15px; text-align: center; }}
        .content {{ background-color: #f5f5f5; padding: 20px; }}
        .request-info {{ background-color: white; padding: 15px; margin: 10px 0; border-right: 4px solid #2196F3; }}
        .button {{ display: inline-block; background-color: #2196F3; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin: 10px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>תזכורת: בקשה ממתינה לאישור</h2>
        </div>
        <div class='content'>
            <p>שלום {manager.FullName ?? manager.UserName},</p>
            
            <p>זוהי תזכורת כי יש לך בקשה הממתינה לאישורך:</p>
            
            <div class='request-info'>
                <p><strong>מספר בקשה:</strong> #{request.Id}</p>
                <p><strong>מבקש:</strong> {requester?.FullName ?? requester?.UserName ?? "לא ידוע"}</p>
                <p><strong>תאריך יצירה:</strong> {request.CreatedAt:dd/MM/yyyy HH:mm}</p>
                <p><strong>ממתין כבר:</strong> {daysPending} ימים ו-{hoursPending % 24} שעות</p>
                {(request.ReminderCount > 0 ? $"<p><strong>מספר תזכורות שנשלחו:</strong> {request.ReminderCount}</p>" : "")}
                {(string.IsNullOrEmpty(request.Comments) ? "" : $"<p><strong>הערות:</strong> {request.Comments}</p>")}
            </div>
            
            <p style='text-align: center;'>
                <a href='{GetRequestUrl(request.Id)}' class='button'>לצפייה ואישור הבקשה</a>
            </p>
            
            <p>אנא אשר או דחה את הבקשה בהקדם האפשרי.</p>
        </div>
        <div class='footer'>
            <p>זוהי הודעה אוטומטית ממערכת הרשאות. אנא אל תשיב להודעה זו.</p>
        </div>
    </div>
</body>
</html>";

            await emailService.SendEmailAsync(manager.Email, subject, body, isHtml: true);
        }

        private string GetRequestUrl(int requestId)
        {
            // This should be the actual URL of your application
            // You might want to get this from configuration
            var baseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "http://localhost:5000";
            return $"{baseUrl}/Requests/Details/{requestId}";
        }
    }
}

