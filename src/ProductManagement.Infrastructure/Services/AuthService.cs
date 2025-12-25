using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;
using System.Security.Claims;

namespace ProductManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        ILogger<AuthService> logger,
        IOptions<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

            // Check if email already exists
            var emailExists = await _unitOfWork.Users.AnyAsync(
                u => u.Email == request.Email.ToLowerInvariant(),
                cancellationToken);

            if (emailExists)
            {
                return Result<AuthResponse>.Failure("Email is already registered");
            }

            // Check if username already exists
            var usernameExists = await _unitOfWork.Users.AnyAsync(
                u => u.UserName == request.UserName.ToLowerInvariant(),
                cancellationToken);

            if (usernameExists)
            {
                return Result<AuthResponse>.Failure("Username is already taken");
            }

            // Hash password
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // Create user
            var user = User.Create(
                request.Email,
                request.UserName,
                passwordHash,
                request.FirstName,
                request.LastName,
                request.PhoneNumber);

            // Assign default Customer role
            var customerRole = await _unitOfWork.Roles.FindAsync(
                r => r.Name == Role.Names.Customer,
                cancellationToken);

            var role = customerRole.FirstOrDefault();
            if (role != null)
            {
                user.AddRole(role);
            }

            // Save user
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User registered successfully: {UserId}", user.Id);

            // Generate tokens
            var roles = new[] { Role.Names.Customer };
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            user.SetRefreshToken(
                refreshToken,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO
            var userDto = _mapper.Map<UserDto>(user);
            userDto = userDto with { Roles = roles.ToList() };

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = userDto
            };

            return Result<AuthResponse>.Success(response, "Registration successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return Result<AuthResponse>.Failure("Registration failed", ex.Message);
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Login attempt for: {EmailOrUsername}", request.EmailOrUsername);

            // Find user by email or username
            var users = await _unitOfWork.Users.FindAsync(
                u => u.Email == request.EmailOrUsername.ToLowerInvariant() ||
                     u.UserName == request.EmailOrUsername.ToLowerInvariant(),
                cancellationToken);

            var user = users.FirstOrDefault();

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found - {EmailOrUsername}", request.EmailOrUsername);
                return Result<AuthResponse>.Failure("Invalid email/username or password");
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
                return Result<AuthResponse>.Failure("Invalid email/username or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User is inactive - {UserId}", user.Id);
                return Result<AuthResponse>.Failure("Account is inactive. Please contact support.");
            }

            // Get user roles
            var userWithRoles = await _unitOfWork.Users.GetWithRolesAsync(
                user.Id,
                cancellationToken);

            var roles = userWithRoles!.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList();

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update last login and refresh token
            user.UpdateLastLogin();
            user.SetRefreshToken(
                refreshToken,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            // Map to DTO
            var userDto = _mapper.Map<UserDto>(user);
            userDto = userDto with { Roles = roles };

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = userDto
            };

            return Result<AuthResponse>.Success(response, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return Result<AuthResponse>.Failure("Login failed", ex.Message);
        }
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to refresh token");

            // Get principal from expired token
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                return Result<AuthResponse>.Failure("Invalid access token");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Result<AuthResponse>.Failure("Invalid token claims");
            }

            // Get user
            var user = await _unitOfWork.Users.GetWithRolesAsync(
                userId,
                cancellationToken);

            if (user == null)
            {
                return Result<AuthResponse>.Failure("User not found");
            }

            // Verify refresh token
            if (user.RefreshToken != request.RefreshToken)
            {
                return Result<AuthResponse>.Failure("Invalid refresh token");
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result<AuthResponse>.Failure("Refresh token has expired");
            }

            // Get user roles
            var roles = user.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList();

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update refresh token
            user.SetRefreshToken(
                newRefreshToken,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);

            // Map to DTO
            var userDto = _mapper.Map<UserDto>(user);
            userDto = userDto with { Roles = roles };

            var response = new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = userDto
            };

            return Result<AuthResponse>.Success(response, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Result<AuthResponse>.Failure("Token refresh failed", ex.Message);
        }
    }

    public async Task<Result> RevokeTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            user.RevokeRefreshToken();
            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token revoked for user: {UserId}", userId);
            return Result.Success("Token revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token for user: {UserId}", userId);
            return Result.Failure("Token revocation failed", ex.Message);
        }
    }

    public async Task<Result<UserDto>> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetWithRolesAsync(
                userId,
                cancellationToken);

            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var roles = user.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList();

            var userDto = _mapper.Map<UserDto>(user);
            userDto = userDto with { Roles = roles };

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user: {UserId}", userId);
            return Result<UserDto>.Failure("Failed to retrieve profile", ex.Message);
        }
    }

    public async Task<Result<UserDto>> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            user.UpdateProfile(
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                userId.ToString());

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile updated for user: {UserId}", userId);

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto, "Profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user: {UserId}", userId);
            return Result<UserDto>.Failure("Failed to update profile", ex.Message);
        }
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            // Verify current password
            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return Result.Failure("Current password is incorrect");
            }

            // Hash new password
            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            // Update password
            user.ChangePassword(newPasswordHash, userId.ToString());

            // Revoke refresh token for security
            user.RevokeRefreshToken();

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password changed for user: {UserId}", userId);
            return Result.Success("Password changed successfully. Please login again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return Result.Failure("Failed to change password", ex.Message);
        }
    }
}