using System;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace Frametric.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly IResend? _resend;
    private readonly string _fromEmail;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly IConfiguration _configuration;

    public ResendEmailService(IConfiguration configuration, ILogger<ResendEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var apiKey = configuration["Resend:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _resend = ResendClient.Create(apiKey);
        }
        else
        {
            _logger.LogWarning("Resend:ApiKey is not configured. Email service will run in log-only audit mode.");
        }

        _fromEmail = configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initiating password reset email to {Email}", toEmail);
        
        if (_resend == null)
        {
            _logger.LogWarning("[MOCK EMAIL] Password Reset Link: {Link}", resetLink);
            return;
        }

        var message = new EmailMessage
        {
            From = _fromEmail,
            To = toEmail,
            Subject = "Frametric - Password Reset",
            HtmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"></head>
            <body style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; padding: 20px;">
                <h2 style="color: #e2ba64;">Frametric</h2>
                <p>We received a request to reset your password.</p>
                <a href="{resetLink}" style="display: inline-block; padding: 12px 24px; background: #e2ba64; color: #000; text-decoration: none; border-radius: 6px; font-weight: bold;">Reset Password</a>
                <p style="margin-top: 24px; color: #666; font-size: 14px;">This link expires in 1 hour. If you didn't request this, ignore this email.</p>
            </body>
            </html>
            """
        };

        await _resend.EmailSendAsync(message, cancellationToken);
    }

    public async Task SendPromotionNotificationAsync(string promotedUsername, string promotedEmail, string newRole, string promotedBy)
    {
        var targetEmail = Environment.GetEnvironmentVariable("PROMOTION_NOTIFICATION_EMAIL") 
            ?? _configuration["Email:PromotionNotificationEmail"];

        _logger.LogInformation("[AUDIT] User {Username} ({Email}) was promoted to {Role} by {PromotedBy}.", 
            promotedUsername, promotedEmail, newRole, promotedBy);

        if (string.IsNullOrWhiteSpace(targetEmail))
        {
            _logger.LogWarning("PROMOTION_NOTIFICATION_EMAIL is not configured. Skipping email dispatch.");
            return;
        }

        if (_resend == null)
        {
            _logger.LogInformation("[MOCK EMAIL / AUDIT LOGGED] To: {TargetEmail} - Privilege Escalation - User {Username} promoted to {Role} by {PromotedBy}", 
                targetEmail, promotedUsername, newRole, promotedBy);
            return;
        }

        var subject = $"[Frametric Security Audit] User Promoted to {newRole}";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head><meta charset=""utf-8""></head>
            <body style=""font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 20px; background: #0c0d10; color: #fff;"">
                <h2 style=""color: #e2ba64; border-bottom: 1px solid #333; padding-bottom: 10px;"">Frametric Security Alert</h2>
                <p>A user privilege escalation event has occurred:</p>
                <table style=""width: 100%; border-collapse: collapse; margin-top: 15px;"">
                    <tr>
                        <td style=""padding: 8px; font-weight: bold; color: #999;"">Promoted User:</td>
                        <td style=""padding: 8px; color: #fff;"">{promotedUsername} ({promotedEmail})</td>
                    </tr>
                    <tr>
                        <td style=""padding: 8px; font-weight: bold; color: #999;"">New Role:</td>
                        <td style=""padding: 8px; color: #e2ba64; font-weight: bold;"">{newRole}</td>
                    </tr>
                    <tr>
                        <td style=""padding: 8px; font-weight: bold; color: #999;"">Actioned By:</td>
                        <td style=""padding: 8px; color: #fff;"">{promotedBy}</td>
                    </tr>
                    <tr>
                        <td style=""padding: 8px; font-weight: bold; color: #999;"">Timestamp:</td>
                        <td style=""padding: 8px; color: #fff;"">{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</td>
                    </tr>
                </table>
                <p style=""margin-top: 30px; font-size: 12px; color: #666; border-top: 1px solid #333; padding-top: 15px;"">
                    If this action was unauthorized, please inspect audit logs immediately.
                </p>
            </body>
            </html>";

        try
        {
            var message = new EmailMessage
            {
                From = _fromEmail,
                To = targetEmail,
                Subject = subject,
                HtmlBody = body
            };

            await _resend.EmailSendAsync(message);
            _logger.LogInformation("Promotion notification email sent successfully via Resend to {TargetEmail}.", targetEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send promotion email via Resend to {TargetEmail}.", targetEmail);
        }
    }
}
