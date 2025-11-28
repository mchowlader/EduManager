
namespace EduSystem.Identity.Application.IService;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(
         string toEmail,
         string userName,
         string resetToken,
         string? customBody = null,
         CancellationToken cancellationToken = default);

    Task<bool> SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string tenantName,
        string? customBody = null,
        CancellationToken cancellationToken = default);
}
