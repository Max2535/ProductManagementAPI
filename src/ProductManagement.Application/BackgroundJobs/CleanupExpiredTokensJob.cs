using Microsoft.Extensions.Logging;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Application.BackgroundJobs;

/// <summary>
/// Background job to clean up expired refresh tokens
/// </summary>
public class CleanupExpiredTokensJob
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CleanupExpiredTokensJob> _logger;

    public CleanupExpiredTokensJob(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<CleanupExpiredTokensJob> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute()
    {
        _logger.LogInformation("Executing Cleanup Expired Tokens Job at {Time}", DateTime.UtcNow);

        try
        {
            var allUsers = await _userRepository.GetAllAsync();
            var expiredUsers = allUsers
                .Where(u => u.RefreshTokenExpiryTime.HasValue &&
                           u.RefreshTokenExpiryTime.Value < DateTime.UtcNow &&
                           !string.IsNullOrEmpty(u.RefreshToken))
                .ToList();

            _logger.LogInformation("Found {Count} users with expired tokens", expiredUsers.Count);

            foreach (var user in expiredUsers)
            {
                user.RevokeRefreshToken();
                await _userRepository.UpdateAsync(user);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired refresh tokens", expiredUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Cleanup Expired Tokens Job");
            throw;
        }
    }
}