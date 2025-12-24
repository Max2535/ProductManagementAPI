using System.Net;
using System.Text.Json;
using ProductManagement.Application.Common;
using ProductManagement.Application.Exceptions;

namespace ProductManagement.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// Best Practice: Centralized error handling
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException notFoundEx => CreateResponse(
                HttpStatusCode.NotFound,
                "Resource not found",
                notFoundEx.Message),

            ValidationException validationEx => CreateResponse(
                HttpStatusCode.BadRequest,
                "Validation error",
                validationEx.Errors),

            BusinessRuleException businessEx => CreateResponse(
                HttpStatusCode.BadRequest,
                "Business rule violation",
                businessEx.Message),

            DuplicateException duplicateEx => CreateResponse(
                HttpStatusCode.Conflict,
                "Duplicate resource",
                duplicateEx.Message),

            _ => CreateResponse(
                HttpStatusCode.InternalServerError,
                "An internal server error occurred",
                exception.Message)
        };

        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response.Body));
    }

    private static (HttpStatusCode StatusCode, object Body) CreateResponse(
        HttpStatusCode statusCode,
        string message,
        string error)
    {
        return (statusCode, new
        {
            success = false,
            message,
            errors = new[] { error },
            timestamp = DateTime.UtcNow
        });
    }

    private static (HttpStatusCode StatusCode, object Body) CreateResponse(
        HttpStatusCode statusCode,
        string message,
        List<string> errors)
    {
        return (statusCode, new
        {
            success = false,
            message,
            errors,
            timestamp = DateTime.UtcNow
        });
    }
}