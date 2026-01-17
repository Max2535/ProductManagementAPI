using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Models;
using ProductManagement.Domain.Entities;
using ProductManagement.Infrastructure.Data;
using Xunit;

namespace ProductManagement.IntegrationTests.Helpers;

/// <summary>
/// Base class for integration tests
/// Provides common setup and helper methods
/// </summary>
[Collection("Integration")]
public abstract class IntegrationTestBase
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly ApplicationDbContext DbContext;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();

        // Get DbContext for test data setup
        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// Register a test user and return authentication token
    /// </summary>
    protected async Task<string> GetAuthTokenAsync(
        string email = "test@example.com",
        string userName = "testuser",
        string password = "Test@123456")
    {
        var registerRequest = new RegisterRequest
        {
            Email = email,
            UserName = userName,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+66812345678"
        };

        var response = await Client.PostAsJsonAsync("/api/Auth/register", registerRequest);

        if (!response.IsSuccessStatusCode)
        {
            // Try to login if already registered
            var loginRequest = new LoginRequest
            {
                EmailOrUsername = email,
                Password = password
            };

            response = await Client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        }

        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return authResponse!.Data!.AccessToken;
    }

    /// <summary>
    /// Set authorization header with bearer token
    /// </summary>
    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Clear database for test isolation
    /// </summary>
    protected void ClearDatabase()
    {
        DbContext.Products.RemoveRange(DbContext.Products);
        DbContext.Categories.RemoveRange(DbContext.Categories);
        DbContext.Users.RemoveRange(DbContext.Users.Where(u => !u.IsDeleted));
        DbContext.SaveChanges();
    }

    /// <summary>
    /// Create test category
    /// </summary>
    protected Guid CreateTestCategory(string name = "Test Category")
    {
        var category = Category.Create(
            name,
            "Test Description",
            null,
            "System");
        DbContext.Categories.Add(category);
        DbContext.SaveChanges();
        return category.Id;
    }
}