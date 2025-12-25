using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;

namespace ProductManagement.Infrastructure.Services;

/// <summary>
/// Email service using SMTP
/// Best Practice: Abstract email sending for easy provider switching
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<Result> SendEmailAsync(
        EmailRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSmtpClient();
            using var message = CreateMailMessage(request);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", request.To));
            return Result.Success("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipients}", string.Join(", ", request.To));
            return Result.Failure("Failed to send email", ex.Message);
        }
    }

    public async Task<Result> SendWelcomeEmailAsync(
        string email,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var emailRequest = new EmailRequest
        {
            To = new List<string> { email },
            Subject = "Welcome to Product Management System!",
            Body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Welcome, {userName}!</h2>
                    <p>Thank you for registering with our Product Management System.</p>
                    <p>You can now start managing your products and inventory.</p>
                    <br>
                    <p>Best regards,</p>
                    <p><strong>Product Management Team</strong></p>
                </body>
                </html>",
            IsHtml = true
        };

        return await SendEmailAsync(emailRequest, cancellationToken);
    }

    public async Task<Result> SendPasswordResetEmailAsync(
        string email,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        var emailRequest = new EmailRequest
        {
            To = new List<string> { email },
            Subject = "Password Reset Request",
            Body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Password Reset Request</h2>
                    <p>You have requested to reset your password.</p>
                    <p>Your reset token is: <strong>{resetToken}</strong></p>
                    <p>This token will expire in 1 hour.</p>
                    <p>If you did not request this, please ignore this email.</p>
                    <br>
                    <p>Best regards,</p>
                    <p><strong>Product Management Team</strong></p>
                </body>
                </html>",
            IsHtml = true
        };

        return await SendEmailAsync(emailRequest, cancellationToken);
    }

    public async Task<Result> SendLowStockAlertAsync(
        string productName,
        string sku,
        int currentStock,
        int minimumStock,
        CancellationToken cancellationToken = default)
    {
        // In production, this should be sent to inventory managers
        var emailRequest = new EmailRequest
        {
            To = new List<string> { "inventory@example.com" },
            Subject = $"Low Stock Alert - {productName}",
            Body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #d9534f;'>⚠️ Low Stock Alert</h2>
                    <p>The following product is running low on stock:</p>
                    <table style='border-collapse: collapse; margin: 20px 0;'>
                        <tr>
                            <td style='padding: 5px; font-weight: bold;'>Product:</td>
                            <td style='padding: 5px;'>{productName}</td>
                        </tr>
                        <tr>
                            <td style='padding: 5px; font-weight: bold;'>SKU:</td>
                            <td style='padding: 5px;'>{sku}</td>
                        </tr>
                        <tr>
                            <td style='padding: 5px; font-weight: bold;'>Current Stock:</td>
                            <td style='padding: 5px; color: #d9534f;'>{currentStock}</td>
                        </tr>
                        <tr>
                            <td style='padding: 5px; font-weight: bold;'>Minimum Stock:</td>
                            <td style='padding: 5px;'>{minimumStock}</td>
                        </tr>
                    </table>
                    <p>Please reorder this product as soon as possible.</p>
                    <br>
                    <p>Best regards,</p>
                    <p><strong>Product Management System</strong></p>
                </body>
                </html>",
            IsHtml = true
        };

        return await SendEmailAsync(emailRequest, cancellationToken);
    }

    private SmtpClient CreateSmtpClient()
    {
        return new SmtpClient
        {
            Host = _emailSettings.SmtpServer,
            Port = _emailSettings.SmtpPort,
            EnableSsl = _emailSettings.EnableSsl,
            Credentials = new NetworkCredential(
                _emailSettings.Username,
                _emailSettings.Password)
        };
    }

    private MailMessage CreateMailMessage(EmailRequest request)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = request.Subject,
            Body = request.Body,
            IsBodyHtml = request.IsHtml
        };

        // Add recipients
        foreach (var to in request.To)
        {
            message.To.Add(to);
        }

        // Add CC
        if (request.Cc != null)
        {
            foreach (var cc in request.Cc)
            {
                message.CC.Add(cc);
            }
        }

        // Add BCC
        if (request.Bcc != null)
        {
            foreach (var bcc in request.Bcc)
            {
                message.Bcc.Add(bcc);
            }
        }

        // Add attachments
        if (request.Attachments != null)
        {
            foreach (var attachment in request.Attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                message.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
            }
        }

        return message;
    }
}