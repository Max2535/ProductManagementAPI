using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<UserDto>> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<UserDto>> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}