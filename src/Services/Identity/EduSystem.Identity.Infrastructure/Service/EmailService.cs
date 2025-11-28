using System.Net.Mail;
using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Application.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EduSystem.Identity.Infrastructure.Service;

public class EmailService(EmailSettings emailSettings, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings _emailSettings = emailSettings;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task<bool> SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetToken,
        string? customBody = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resetLink = $"baseurl/reset-password?token={resetToken}";
            var subject = "Reset Your Password - EduSystem";

            var body = customBody ?? GetDefaultPasswordResetBody(userName, resetLink);
            var message = BuildEmailMessage(toEmail, subject, body);

            await SendAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string tenantName,
        string? customBody = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var loginUrl = "baseurl/login";
            var subject = $"Welcome to {tenantName} - EduSystem";

            var body = customBody ?? GetDefaultWelcomeBody(userName, tenantName, loginUrl);
            var message = BuildEmailMessage(toEmail, subject, body);

            await SendAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", toEmail);
            return false;
        }
    }

    private async Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        using var smtp = new MailKit.Net.Smtp.SmtpClient();

        await smtp.ConnectAsync(
            _emailSettings.Host,
            _emailSettings.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await smtp.AuthenticateAsync(
            _emailSettings.UserName,
            _emailSettings.Password,
            cancellationToken);

        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }

    private MimeMessage BuildEmailMessage(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("EduSystem", _emailSettings.UserName));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = htmlBody };
        return message;
    }

    // -------------------------------------------------------
    // PASSWORD RESET HTML
    // -------------------------------------------------------

    private string GetDefaultPasswordResetBody(string userName, string resetLink)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            </head>
            <body style='margin:0;padding:0;font-family:Arial,sans-serif;background:#f5f5f5'>
                <table style='width:100%;border-collapse:collapse'>
                    <tr>
                        <td style='padding:40px 0'>
                            <table style='width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,.1)'>
                                <tr>
                                    <td style='padding:40px'>
                                        <h1 style='margin:0;color:#333;font-size:24px;font-weight:600'>
                                            Password Reset Request
                                        </h1>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding:0 40px 40px;color:#666;font-size:16px;line-height:1.6'>
                                        <p>Hello {userName},</p>
                                        <p>We received a request to reset your password. Click below:</p>

                                        <p style='margin:30px 0'>
                                            <a href='{resetLink}' 
                                               style='padding:14px 32px;background:#4CAF50;color:#fff;text-decoration:none;
                                                      font-weight:600;font-size:16px;border-radius:4px'>
                                                Reset Password
                                            </a>
                                        </p>

                                        <p>Or copy this link:</p>
                                        <p style='word-break:break-all;color:#4CAF50;font-size:14px'>{resetLink}</p>

                                        <div style='margin-top:30px;padding:16px;background:#fff3cd;border-left:4px solid #ffc107;border-radius:4px'>
                                            <p style='margin:0;color:#856404;font-size:14px'>
                                                <strong>Important:</strong> This link will expire in 1 hour.
                                            </p>
                                        </div>

                                        <p style='margin-top:24px;font-size:14px;color:#999'>
                                            If you didn't request this, ignore this email.
                                        </p>
                                    </td>
                                </tr>

                                <tr>
                                    <td style='padding:20px 40px;background:#f9f9f9;border-top:1px solid #eee;text-align:center;color:#999;font-size:12px'>
                                        EduSystem - Education Management System
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
    }

    // -------------------------------------------------------
    // WELCOME EMAIL HTML
    // -------------------------------------------------------

    private string GetDefaultWelcomeBody(string userName, string tenantName, string loginUrl)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            </head>
            <body style='margin:0;padding:0;font-family:Arial,sans-serif;background:#f5f5f5'>
                <table style='width:100%;border-collapse:collapse'>
                    <tr>
                        <td style='padding:40px 0'>
                            <table style='width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,.1)'>
                                <tr>
                                    <td style='padding:40px;text-align:center'>
                                        <h1 style='margin:0;color:#333;font-size:28px;font-weight:600'>
                                            Welcome to {tenantName}!
                                        </h1>
                                    </td>
                                </tr>

                                <tr>
                                    <td style='padding:0 40px 40px;color:#666;font-size:16px;line-height:1.6'>
                                        <p>Hello {userName},</p>
                                        <p>Your account has been created. We're excited to have you with us.</p>

                                        <p style='margin:30px 0'>
                                            <a href='{loginUrl}' 
                                                style='padding:14px 32px;background:#667eea;color:#fff;text-decoration:none;
                                                        font-weight:600;font-size:16px;border-radius:4px'>
                                                Login to Your Account
                                            </a>
                                        </p>

                                        <p style='margin-top:24px;font-size:14px;color:#999'>
                                            If you need help, contact support anytime.
                                        </p>
                                    </td>
                                </tr>

                                <tr>
                                    <td style='padding:20px 40px;background:#f9f9f9;border-top:1px solid #eee;text-align:center;color:#999;font-size:12px'>
                                        EduSystem - Education Management System
                                    </td>
                                </tr>

                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
    }
}
