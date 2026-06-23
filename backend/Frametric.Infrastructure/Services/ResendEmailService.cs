using Frametric.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Resend;

namespace Frametric.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly string _fromEmail;

    public ResendEmailService(IConfiguration configuration)
    {
        var apiKey = configuration["Resend:ApiKey"]
            ?? throw new InvalidOperationException("Resend:ApiKey is not configured");
        _resend = ResendClient.Create(apiKey);
        _fromEmail = configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
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
}
