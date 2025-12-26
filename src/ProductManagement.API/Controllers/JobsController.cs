using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.BackgroundJobs;
using ProductManagement.Application.Common;

namespace ProductManagement.API.Controllers;

/// <summary>
/// Background Jobs Controller (Admin Only)
/// For testing and manual job triggering
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly ILogger<JobsController> _logger;

    public JobsController(ILogger<JobsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Manually trigger stock alert job
    /// </summary>
    [HttpPost("trigger-stock-alert")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult TriggerStockAlert()
    {
        var jobId = BackgroundJob.Enqueue<ProductStockAlertJob>(job => job.Execute());

        _logger.LogInformation("Stock alert job triggered manually. Job ID: {JobId}", jobId);

        return Ok(ApiResponse<object>.SuccessResponse(
            new { jobId },
            "Stock alert job queued successfully"));
    }

    /// <summary>
    /// Manually trigger cleanup tokens job
    /// </summary>
    [HttpPost("trigger-cleanup-tokens")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult TriggerCleanupTokens()
    {
        var jobId = BackgroundJob.Enqueue<CleanupExpiredTokensJob>(job => job.Execute());

        _logger.LogInformation("Cleanup tokens job triggered manually. Job ID: {JobId}", jobId);

        return Ok(ApiResponse<object>.SuccessResponse(
            new { jobId },
            "Cleanup tokens job queued successfully"));
    }

    /// <summary>
    /// Schedule a delayed job (example)
    /// </summary>
    [HttpPost("schedule-delayed-job")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult ScheduleDelayedJob([FromQuery] int delayMinutes = 5)
    {
        var jobId = BackgroundJob.Schedule<ProductStockAlertJob>(
            job => job.Execute(),
            TimeSpan.FromMinutes(delayMinutes));

        _logger.LogInformation(
            "Stock alert job scheduled to run in {Minutes} minutes. Job ID: {JobId}",
            delayMinutes,
            jobId);

        return Ok(ApiResponse<object>.SuccessResponse(
            new { jobId, scheduledFor = DateTime.UtcNow.AddMinutes(delayMinutes) },
            $"Job scheduled to run in {delayMinutes} minutes"));
    }

    /// <summary>
    /// Get job status
    /// </summary>
    [HttpGet("status/{jobId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetJobStatus(string jobId)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var jobDetails = monitoringApi.JobDetails(jobId);

        if (jobDetails == null)
        {
            return NotFound(ApiResponse<object>.FailureResponse("Job not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            jobId,
            state = jobDetails.History.FirstOrDefault()?.StateName,
            createdAt = jobDetails.CreatedAt,
            properties = jobDetails.Properties
        }));
    }
}