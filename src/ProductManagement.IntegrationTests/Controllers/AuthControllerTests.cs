using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.IntegrationTests.Helpers;
using Xunit;

namespace ProductManagement.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for Authentication endpoints
/// </summary>
public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var request = TestDataGenerator.CreateValidRegisterRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.User.Email.Should().Be(request.Email);
        result.Data.User.Roles.Should().Contain("Customer");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = TestDataGenerator.CreateValidRegisterRequest("duplicate@example.com");
        await Client.PostAsJsonAsync("/api/Auth/register", request);

        // Act - try to register again with same email
        var response = await Client.PostAsJsonAsync("/api/Auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("already registered");
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = TestDataGenerator.CreateValidRegisterRequest();
        request = request with { Password = "weak", ConfirmPassword = "weak" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200OK()
    {
        // Arrange
        var registerRequest = TestDataGenerator.CreateValidRegisterRequest();
        await Client.PostAsJsonAsync("/api/Auth/register", registerRequest);

        var loginRequest = TestDataGenerator.CreateValidLoginRequest(
            registerRequest.Email,
            registerRequest.Password);

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.User.Email.Should().Be(registerRequest.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401Unauthorized()
    {
        // Arrange
        var loginRequest = TestDataGenerator.CreateValidLoginRequest(
            "nonexistent@example.com",
            "WrongPassword123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WithValidToken_ShouldReturn200OK()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        // Act
        var response = await Client.GetAsync("/api/Auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ShouldReturn401Unauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/Auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ShouldReturn200OK()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var updateRequest = new UpdateProfileRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            PhoneNumber = "+66987654321"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/Auth/profile", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.FirstName.Should().Be("Updated");
        result.Data.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task ChangePassword_WithValidData_ShouldReturn200OK()
    {
        // Arrange
        var email = "changepass@example.com";
        var oldPassword = "OldPassword@123";
        var newPassword = "NewPassword@123";

        var token = await GetAuthTokenAsync(email, "changepassuser", oldPassword);
        SetAuthorizationHeader(token);

        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = oldPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/change-password", changePasswordRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify can login with new password
        var loginRequest = new LoginRequest
        {
            EmailOrUsername = email,
            Password = newPassword
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_WithValidTokens_ShouldReturn200OK()
    {
        // Arrange
        var registerResponse = await Client.PostAsJsonAsync(
            "/api/Auth/register",
            TestDataGenerator.CreateValidRegisterRequest());

        var authResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = authResult!.Data!.AccessToken,
            RefreshToken = authResult.Data.RefreshToken
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.AccessToken.Should().NotBe(authResult.Data.AccessToken); // New token
    }

    [Fact]
    public async Task Logout_WithValidToken_ShouldReturn200OK()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        // Act
        var response = await Client.PostAsync("/api/Auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}