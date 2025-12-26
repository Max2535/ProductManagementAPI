using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;

namespace ProductManagement.API.Controllers;

/// <summary>
/// Email Testing Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
[Produces("application/json")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(
        IEmailService emailService,
        ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Send test email
    /// </summary>
    [HttpPost("send-test")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendTestEmail(
        [FromBody] SendTestEmailRequest request,
        CancellationToken cancellationToken)
    {
        var emailRequest = new EmailRequest
        {
            To = new List<string> { request.To },
            Subject = "Test Email from Product Management API",
            Body = @"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Test Email</h2>
                    <p>This is a test email from the Product Management API.</p>
                    <p>If you received this, the email service is working correctly! ✅</p>
                    <br>
                    <p>Best regards,</p>
                    <p><strong>Product Management System</strong></p>
                </body>
                </html>",
            IsHtml = true
        };

        var result = await _emailService.SendEmailAsync(emailRequest, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<object>.SuccessResponse(null!, result.Message))
            : BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));
    }

    /// <summary>
    /// Send welcome email
    /// </summary>
    [HttpPost("send-welcome")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendWelcomeEmail(
        [FromBody] SendWelcomeEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _emailService.SendWelcomeEmailAsync(
            request.Email,
            request.UserName,
            cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<object>.SuccessResponse(null!, result.Message))
            : BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));
    }

    /// <summary>
    /// Send low stock alert email
    /// </summary>
    [HttpPost("send-low-stock-alert")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendLowStockAlert(
        [FromBody] SendLowStockAlertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _emailService.SendLowStockAlertAsync(
            request.ProductName,
            request.SKU,
            request.CurrentStock,
            request.MinimumStock,
            cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<object>.SuccessResponse(null!, result.Message))
            : BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));
    }
}

public record SendTestEmailRequest
{
    public string To { get; init; } = string.Empty;
}

public record SendWelcomeEmailRequest
{
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
}

public record SendLowStockAlertRequest
{
    public string ProductName { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public int CurrentStock { get; init; }
    public int MinimumStock { get; init; }
}