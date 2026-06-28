using System.Threading.Tasks;

namespace Frametric.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
    Task SendPromotionNotificationAsync(string promotedUsername, string promotedEmail, string newRole, string promotedBy);
}
