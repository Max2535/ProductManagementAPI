using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Interfaces;

public interface IEmailService
{
    Task<Result> SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default);
    Task<Result> SendWelcomeEmailAsync(string email, string userName, CancellationToken cancellationToken = default);
    Task<Result> SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default);
    Task<Result> SendLowStockAlertAsync(string productName, string sku, int currentStock, int minimumStock, CancellationToken cancellationToken = default);
}